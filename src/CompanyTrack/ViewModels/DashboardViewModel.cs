using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CompanyTrack.Core.Models;
using CompanyTrack.Core.Rules;

namespace CompanyTrack.ViewModels;

public class PriorityRow
{
    public string Perusahaan { get; init; } = "";
    public string StatusLabel { get; init; } = "";
    public IBrush StatusBg { get; init; } = Palette.Gray;
    public IBrush StatusFg { get; init; } = Palette.GrayFg;
    public string Reason { get; init; } = "";
    public string Pic { get; init; } = "";
    public IRelayCommand OpenCommand { get; init; } = null!;
}

public class FunnelRow
{
    public string Label { get; init; } = "";
    public int Jumlah { get; init; }
    public double Max { get; init; }
}

public partial class DashboardViewModel : ViewModelBase
{
    private readonly INavigator _nav;

    public DashboardViewModel(AppServices services, INavigator nav) : base(services) => _nav = nav;

    [ObservableProperty] private bool _hasPeriode;
    [ObservableProperty] private string _periodeText = "";
    [ObservableProperty] private string _countdownText = "";
    [ObservableProperty] private int _followUpToday;
    [ObservableProperty] private int _followUpOverdue;
    [ObservableProperty] private int _staleCount;
    [ObservableProperty] private string _staleHint = "";
    [ObservableProperty] private string _confirmedText = "";
    [ObservableProperty] private double _confirmedValue;
    [ObservableProperty] private double _confirmedMax = 1;

    public List<PriorityRow> Priority { get; private set; } = new();
    public List<FunnelRow> Funnel { get; private set; } = new();
    public List<string> Activities { get; private set; } = new();

    public bool NoPriority => Priority.Count == 0;

    public override async Task LoadAsync()
    {
        await RunAsync(async () =>
        {
            var d = await S.Dashboard.LoadAsync();
            var now = DateTime.Now;
            HasPeriode = d.Periode is not null;
            if (d.Periode is not null)
            {
                PeriodeText = $"{d.Periode.Nama}, kunjungan {Fmt.Date(d.Periode.TanggalKunjungan)}";
                CountdownText = d.DaysToEvent >= 0 ? $"H-{d.DaysToEvent} menuju hari kunjungan" : $"Hari kunjungan sudah lewat {-d.DaysToEvent} hari";
            }
            FollowUpToday = d.FollowUpToday;
            FollowUpOverdue = d.FollowUpOverdue;
            StaleCount = d.StaleCount;
            StaleHint = $"Tanpa aktivitas lebih dari {d.StaleThresholdDays} hari";
            ConfirmedText = $"{d.Confirmed} dari {d.Target}";
            ConfirmedMax = Math.Max(1, d.Target);
            ConfirmedValue = d.Confirmed;

            Priority = d.Priority.Select(p =>
            {
                var (bg, fg) = Palette.ForStatus(p.Status);
                int days = StaleChecker.DaysSince(p.TanggalAktivitasTerakhir, now);
                string reason = p.NextFollowUp is not null && p.NextFollowUp.Value <= DateOnly.FromDateTime(now)
                    ? "Follow-up: " + FollowUpClassifier.DueLabel(p.NextFollowUp.Value, DateOnly.FromDateTime(now))
                    : $"Tidak ada aktivitas {days} hari";
                int id = p.PengajuanId;
                return new PriorityRow
                {
                    Perusahaan = p.Perusahaan, StatusLabel = p.Status.ToLabel(), StatusBg = bg, StatusFg = fg,
                    Reason = reason, Pic = p.PicUsername,
                    OpenCommand = new AsyncRelayCommand(() => _nav.OpenDetailAsync(id))
                };
            }).ToList();

            double max = Math.Max(1, d.Funnel.Count == 0 ? 1 : d.Funnel.Max(f => f.Jumlah));
            Funnel = d.Funnel.Select(f => new FunnelRow { Label = f.Status.ToLabel(), Jumlah = f.Jumlah, Max = max }).ToList();
            Activities = d.Activities.Select(a => $"{a.UserName}: {a.Keterangan} ({Fmt.DateTimeShort(a.Waktu)})").ToList();

            OnPropertyChanged(nameof(Priority));
            OnPropertyChanged(nameof(Funnel));
            OnPropertyChanged(nameof(Activities));
            OnPropertyChanged(nameof(NoPriority));
        });
    }
}
