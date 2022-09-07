using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Words
{
    internal class MainViewModel : INotifyPropertyChanged
    {
        private DateTime _selectedDate = DateTime.Today;
        private WordLists _selectedWordList = null;
        private DateTime _planStartDate = DateTime.Today;

        public event PropertyChangedEventHandler PropertyChanged;

        public DateTime SelectedDate
        {
            get => _selectedDate;
            set
            {
                var backup = _selectedDate;
                if (_selectedDate != value)
                {
                    _selectedDate = value;
                    if (PropertyChanged != null)
                        PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedDate)));
                }
                int index = (int)DateTime.Parse(SelectedDate.ToString("yyyy-MM-dd")).Subtract(PlanStartDate).TotalDays;
                var results = WordLists.Where(list => list.Id == index + 1);
                if (results.Count() != 1)
                    SelectedDate = backup;
                else
                    SelectedWordList = results.First();
            }
        }
        public WordLists SelectedWordList
        {
            get => _selectedWordList;
            private set
            {
                if (_selectedWordList != value)
                {
                    _selectedWordList = value;
                    if (PropertyChanged != null)
                        PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedWordList)));
                }
            }
        }
        public IList<WordLists> WordLists { get; private set; }
        public IList<Schedules> Schedules { get; private set; }
        public DateTime PlanStartDate
        {
            get => _planStartDate;
            set
            {
                if (_planStartDate != value)
                {
                    _planStartDate = value;
                    if (PropertyChanged != null)
                        PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(PlanStartDate)));
                    Task.Run(async () =>
                    {
                        await SecureStorage.Default.SetAsync("PlanStartDate", _planStartDate.ToString("yyyy-MM-dd"));
                    });
                }
            }
        }

        public MainViewModel()
        {
            WordLists = new List<WordLists>();
            Schedules = new List<Schedules>();

            // Read plan start date.
            Task planStartDate = Task.Run(async () =>
            {
                var startDate = await SecureStorage.Default.GetAsync("PlanStartDate");
                if (startDate == null)
                {
                    startDate = DateTime.Today.ToString("yyyy-MM-dd");
                    await SecureStorage.Default.SetAsync("PlanStartDate", startDate);
                }
                PlanStartDate = DateTime.Parse(startDate);
            });

            // Load word lists.
            Task wordLists = Task.Run(async () =>
            {
                using var stream = await FileSystem.OpenAppPackageFileAsync("IELTS.txt");
                using var reader = new StreamReader(stream);

                string line;
                WordLists wordList = new();
                while ((line = reader.ReadLine()) != null)
                {
                    if (line == string.Empty)
                        continue;
                    if (line.StartsWith("Word List"))
                    {
                        if (wordList.Id != 0)
                            WordLists.Add(wordList);
                        wordList = new() { Id = int.Parse(line.Substring(10).Trim()) };
                    }
                    else
                    {
                        int i;
                        for (i = 0; i < line.Length; i++)
                        {
                            if (line[i] == '[' || line[i] == '/' || !char.IsAscii(line[i]))
                                break;
                        }
                        wordList.Words.Add(new Words() { Word = line.Substring(0, i).Trim(), Description = line.Substring(i).Trim() });
                    }
                }
                WordLists.Add(wordList);
            });

            // Load schedules.
            Task schedules = Task.Run(async () =>
            {
                using var stream = await FileSystem.OpenAppPackageFileAsync("Schedule.txt");
                using var reader = new StreamReader(stream);
                var contents = reader.ReadToEnd();

                foreach (var day in contents.Split('\n'))
                {
                    var schedule = day.Split(',').Select(str => str.Trim()).ToList();

                    var date = DateTime.Parse(schedule[0]);
                    date = DateTime.Parse("2022-" + date.ToString("MM-dd"));

                    var lists = new List<(ScheduleType, (int, int))>();
                    for (int i = 1; i < schedule.Count; i++)
                    {
                        var list = schedule[i];

                        var type = list.Contains("*") ? ScheduleType.Reviewing : ScheduleType.Learning;
                        var range = list.Substring(list.IndexOf(" ") + 1).Split("~");
                        int from = int.Parse(range[0]);
                        int to = int.Parse(range[1]);

                        lists.Add((type, (from, to)));
                    }

                    Schedules.Add(new Schedules() { Date = date, Lists = lists });
                }
            });

            Task.WaitAll(planStartDate, wordLists, schedules);

            int index = (int)DateTime.Parse(SelectedDate.ToString("yyyy-MM-dd")).Subtract(PlanStartDate).TotalDays;
            var results = WordLists.Where(list => list.Id == index + 1);
            if (results.Count() != 1)
            {
                PlanStartDate = DateTime.Today;
                SelectedDate = DateTime.Today;
            }
            else
                SelectedWordList = results.First();
        }
    }
}
