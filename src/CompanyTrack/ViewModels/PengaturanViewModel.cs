using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CompanyTrack.Core.Models;
using CompanyTrack.Core.Rules;

namespace CompanyTrack.ViewModels;

public partial class PengaturanViewModel : ViewModelBase
{
    public PengaturanViewModel(AppServices services) : base(services) { }

    [ObservableProperty] private string _threshold = "";

    public string AkunText => $"{S.Session.CurrentUser?.Nama} ({S.Session.CurrentUser?.Username}), {S.Session.Role.ToLabel()}";

    public override async Task LoadAsync()
    {
        await RunAsync(async () => Threshold = (await S.Master.GetThresholdAsync()).ToString());
    }

    [RelayCommand]
    private async Task SaveThresholdAsync()
    {
        await RunAsync(async () =>
        {
            if (!int.TryParse(Threshold.Trim(), out int days)) throw new DomainException("Ambang harus berupa angka.");
            await S.Master.SetThresholdAsync(days);
        }, "Pengaturan disimpan.");
    }
}
