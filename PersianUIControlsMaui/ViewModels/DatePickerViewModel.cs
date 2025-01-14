﻿using Microsoft.Maui.Layouts;
using PersianUIControlsMaui.Enums;
using PersianUIControlsMaui.Models;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace PersianUIControlsMaui.ViewModels;

public class DatePickerViewModel : ObservableObject
{
    #region Propertie's

    #region Field's
    private int currentYear;
    private string currentMonth;
    private SelectionDateMode selectDateMode;
    private CalendarOptions options;
    #endregion

    public int CurrentYear { get => currentYear; set => SetProperty(ref currentYear, value); }
    public string CurrentMonth { get => currentMonth; set => SetProperty(ref currentMonth, value); }
    public SelectionDateMode SelectDateMode { get => selectDateMode; set => SetProperty(ref selectDateMode, value); }
    public CalendarOptions Options { get => options; set => SetProperty(ref options, value); }
    #endregion

    #region Collection's

    #region Field's
    private List<string> daysOfWeek;
    private List<DayOfMonth> daysOfMonth;
    private List<DayOfMonth> selectedDays;
    #endregion

    public List<string> DaysOfWeek { get => daysOfWeek; set => SetProperty(ref daysOfWeek, value); }
    public List<DayOfMonth> DaysOfMonth { get => daysOfMonth; set => SetProperty(ref daysOfMonth, value); }
    public List<DayOfMonth> SelectedDays { get => selectedDays; set => SetProperty(ref selectedDays, value); }
    #endregion

    #region Command's

    #region Field's
    private Command nextMonthCommand;
    private Command prevMonthCommand;
    private Command nextYearCommand;
    private Command prevYearCommand;
    private Command initCalendarDaysCommand;
    private Command selectDateCommand;
    #endregion

    public Command NextMonthCommand { get { nextMonthCommand ??= new Command(NextMonth); return nextMonthCommand; } }
    public Command PrevMonthCommand { get { prevMonthCommand ??= new Command(PrevMonth); return prevMonthCommand; } }
    public Command NextYearCommand { get { nextYearCommand ??= new Command(NextYear); return nextYearCommand; } }
    public Command PrevYearCommand { get { prevYearCommand ??= new Command(PrevYear); return prevYearCommand; } }
    public Command InitCalendarDaysCommand { get { initCalendarDaysCommand ??= new Command(InitCalendarDays); return initCalendarDaysCommand; } }
    public Command SelectDateCommand { get { selectDateCommand ??= new Command(SelectDate); return selectDateCommand; } }
    #endregion

    public DatePickerViewModel(CalendarOptions options)
    {
        SelectedDays = new List<DayOfMonth>();
        SelectDateMode = options.SelectDateMode;
        Options = options;

        if (this.DaysOfWeek == null)
            FillDaysOfWeek();

        InitCalendarDays(options.SelectedPersianDate.ToDateTime());
    }

    private void FillDaysOfWeek()
    {
        this.DaysOfWeek = typeof(PersianDayOfWeek).GetMembers()
            .Where(x => x.MemberType == MemberTypes.Field)
            .Select(x => ((DisplayAttribute)x.GetCustomAttribute(typeof(DisplayAttribute)))?.Name)
            .Where(x => !string.IsNullOrEmpty(x)).ToList();
    }

    private void InitCalendarDays(object obj)
    {
        if (obj is not DateTime date)
            return;

        CurrentMonth = Enum.GetNames(typeof(PersianMonthNames))[date.GetPersianMonth() - 1];
        CurrentYear = date.GetPersianYear();

        var firstDayOfMonth = date.GetPersianBeginningMonth().ToDateTime();
        var endDayOfMonth = date.GetPersianEndingMonth().ToDateTime();

        var persianDayOfWeek = (int)firstDayOfMonth.GetPersianDay();
        int startDayMonth = ((persianDayOfWeek + 1) == 7 ? 0 : (persianDayOfWeek + 1));
        var monthDaysCount = Enumerable.Range(-startDayMonth, (endDayOfMonth - firstDayOfMonth).Days + startDayMonth + 1).ToList();

        DaysOfMonth = new List<DayOfMonth>(monthDaysCount.Count);
        DaysOfMonth.AddRange(GetDaysOfMonth(monthDaysCount, firstDayOfMonth, date));
    }

    private List<DayOfMonth> GetDaysOfMonth(List<int> monthDaysCount, DateTime firstDayOfMonth, DateTime date)
    {
        return monthDaysCount.Select(x =>
        {
            var currentDate = firstDayOfMonth.AddDays(x);
            bool isHoliday = currentDate.GetPersianDay() == DayOfWeek.Friday;
            var day = new DayOfMonth()
            {
                DayNum = currentDate.GetPersianDayOfMonth(),
                GregorianDate = currentDate,
                PersianDate = currentDate.ToPersianDate(),
                PersianDateNo = currentDate.ToPersianDate().Replace("/", "").ToInt(),
                IsSelected = GetIsSelected(currentDate, date),
                IsInCurrentMonth = x >= 0,
                IsHoliday = isHoliday,
                DayOfWeek = (PersianDayOfWeek)currentDate.GetPersianDay(),
                CanSelect = GetCanSelect(currentDate),
            };
            return day;
        }).ToList();
    }

    private void SelectDate(object obj)
    {
        if (obj is not DayOfMonth dayOfMonth || !dayOfMonth.CanSelect)
            return;
        if (Options.SelectionMode == Enums.SelectionMode.Single)
            DaysOfMonth.ForEach(day => { day.IsSelected = day.PersianDate == dayOfMonth.PersianDate; });
        else if (Options.SelectionMode == Enums.SelectionMode.Multiple)
            ToggleMultipleDates(dayOfMonth);
        else if (Options.SelectionMode == Enums.SelectionMode.Range)
        {
            if (dayOfMonth.PersianDateNo <= SelectedDays.FirstOrDefault(x => x.IsSelected)?.PersianDateNo)
            {
                DaysOfMonth.ForEach(x => { x.IsSelected = false; x.CanSelect = GetCanSelect(x.GregorianDate); });
                SelectedDays.Clear();
            }
            ToggleRangeDates(dayOfMonth);
        }
    }

    private void ToggleRangeDates(DayOfMonth dayOfMonth)
    {
        if (SelectedDays.Any())
            DaysOfMonth.Where(x => x.PersianDateNo >= dayOfMonth.PersianDateNo).ToList().ForEach(x =>
            {
                x.IsSelected = x.PersianDate == dayOfMonth.PersianDate;
                x.CanSelect = x.IsSelected;

                if (x.IsSelected)
                    SelectedDays.Add(x);
                else if (SelectedDays.Exists(s => s.PersianDateNo == x.PersianDateNo))
                    SelectedDays.Remove(x);
            });

        if (!SelectedDays.Any())
            DaysOfMonth.Where(x => x.PersianDateNo == dayOfMonth.PersianDateNo).ToList().ForEach(x =>
            {
                x.IsSelected = x.PersianDateNo == dayOfMonth.PersianDateNo;
                x.CanSelect = GetCanSelect(x.GregorianDate);

                if (x.IsSelected)
                    SelectedDays.Add(x);
                else if (SelectedDays.Exists(s => s.PersianDateNo == x.PersianDateNo))
                    SelectedDays.Remove(x);
            });
    }

    private void ToggleMultipleDates(DayOfMonth dayOfMonth)
    {
        var selectedDate = DaysOfMonth.FirstOrDefault(x => x.PersianDateNo == dayOfMonth.PersianDateNo);
        if (!selectedDate.IsSelected)
        {
            selectedDate.IsSelected = !selectedDate.IsSelected;
            SelectedDays.Add(selectedDate);
        }
        else
        {
            SelectedDays.RemoveAll(x => x.PersianDateNo == selectedDate.PersianDateNo);
            selectedDate.IsSelected = !selectedDate.IsSelected;
        }
    }

    private bool GetCanSelect(DateTime currentDate)
    {
        bool isHoliday = currentDate.GetPersianDay() == DayOfWeek.Friday;
        return (currentDate >= Options.MinDateCanSelect || Options.MinDateCanSelect is null)
                && (currentDate <= Options.MaxDateCanSelect || Options.MaxDateCanSelect is null)
                && (!isHoliday || Options.CanSelectHolidays);
    }

    private bool GetIsSelected(DateTime currentDate, DateTime date)
    {
        if (Options.SelectionMode == Enums.SelectionMode.Single)
            return date.Date == currentDate.Date;
        if (Options.SelectionMode == Enums.SelectionMode.Multiple || Options.SelectionMode == Enums.SelectionMode.Range)
            return SelectedDays.Exists(x => x.GregorianDate.Date == currentDate.Date);

        return false;
    }

    private void NextMonth(object obj)
    {
        InitCalendarDays(DaysOfMonth.FirstOrDefault(x => x.IsSelected).GregorianDate.AddMonths(1));
    }

    private void PrevMonth(object obj)
    {
        InitCalendarDays(DaysOfMonth.FirstOrDefault(x => x.IsSelected).GregorianDate.AddMonths(-1));
    }

    private void NextYear(object obj)
    {
        InitCalendarDays(DaysOfMonth.FirstOrDefault(x => x.IsSelected).GregorianDate.AddYears(1));
    }

    private void PrevYear(object obj)
    {
        InitCalendarDays(DaysOfMonth.FirstOrDefault(x => x.IsSelected).GregorianDate.AddYears(-1));
    }

    public bool CanClose(DayOfMonth selectedDate)
    {
        return (Options.AutoCloseAfterSelectDate && Options.SelectionMode == Enums.SelectionMode.Range && SelectedDays.Count == 2)
            || selectedDate.CanSelect && Options.AutoCloseAfterSelectDate && Options.SelectionMode == Enums.SelectionMode.Single;
    }
}