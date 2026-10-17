using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CompanyTrack.Core.Models;
using CompanyTrack.Core.Rules;

namespace CompanyTrack.ViewModels;

/// <summary>Item pilihan untuk ComboBox dengan label tampilan.</summary>
public sealed class Opt<T>
{
    public T? Value { get; }
    public string Label { get; }
    public Opt(T? value, string label) { Value = value; Label = label; }
    public override string ToString() => Label;
}

public interface INavigator
{
    Task NavigateAsync(string key);
    Task OpenDetailAsync(int pengajuanId);
    Task RefreshBadgeAsync();
}

public static class Palette
{
    private static IBrush B(string hex) => Brush.Parse(hex);

    public static IBrush Gray = B("#2294A3B8"), GrayFg = B("#94A3B8");
    public static IBrush Blue = B("#3A3B82F6"), BlueFg = B("#60A5FA");
    public static IBrush Amber = B("#3AF59E0B"), AmberFg = B("#F59E0B");
    public static IBrush Violet = B("#3A8B5CF6"), VioletFg = B("#A78BFA");
    public static IBrush Green = B("#3A10B981"), GreenFg = B("#10B981");
    public static IBrush Red = B("#3AEF4444"), RedFg = B("#F87171");

    public static (IBrush bg, IBrush fg) ForStatus(StatusPengajuan s) => s switch
    {
        StatusPengajuan.Dihubungi or StatusPengajuan.SuratTerkirim => (Blue, BlueFg),
        StatusPengajuan.MenungguBalasan => (Amber, AmberFg),
        StatusPengajuan.NegosiasiJadwal => (Violet, VioletFg),
        StatusPengajuan.Dikonfirmasi or StatusPengajuan.Terlaksana => (Green, GreenFg),
        StatusPengajuan.Ditolak => (Red, RedFg),
        _ => (Gray, GrayFg)
    };

    public static (IBrush bg, IBrush fg) ForBucket(FollowUpBucket b) => b switch
    {
        FollowUpBucket.Terlambat => (Red, RedFg),
        FollowUpBucket.HariIni => (Blue, BlueFg),
        _ => (Gray, GrayFg)
    };
}

public static class Fmt
{
    private static readonly string[] Bulan = { "Jan", "Feb", "Mar", "Apr", "Mei", "Jun", "Jul", "Agu", "Sep", "Okt", "Nov", "Des" };

    public static string Date(DateOnly d) => $"{d.Day} {Bulan[d.Month - 1]} {d.Year}";
    public static string Date(DateOnly? d) => d is null ? "-" : Date(d.Value);
    public static string DateTimeShort(DateTime d) => $"{d.Day} {Bulan[d.Month - 1]} {d.Year}, {d:HH:mm}";

    public static DateOnly? ToDateOnly(DateTime? d) => d is null ? null : DateOnly.FromDateTime(d.Value);
    public static DateTime? ToDateTime(DateOnly? d) => d?.ToDateTime(TimeOnly.MinValue);
}

public abstract partial class ViewModelBase : ObservableObject
{
    protected AppServices S { get; }

    protected ViewModelBase(AppServices services) => S = services;

    [ObservableProperty] private bool _isBusy;

    public bool CanWrite => S.Session.CanWrite;
    public bool CanManage => S.Session.CanManage;
    public bool IsReadOnly => !S.Session.CanWrite;

    public virtual Task LoadAsync() => Task.CompletedTask;

    /// <summary>Menjalankan aksi dengan penanganan kesalahan seragam. Mengembalikan true bila berhasil.</summary>
    protected async Task<bool> RunAsync(Func<Task> action, string? successMessage = null)
    {
        try
        {
            IsBusy = true;
            await action();
            if (successMessage is not null) S.Notifier.Success(successMessage);
            return true;
        }
        catch (DomainException ex)
        {
            S.Notifier.Error(ex.Message);
            return false;
        }
        catch (Exception ex)
        {
            S.Notifier.Error("Terjadi kesalahan saat mengakses data. Pastikan database berjalan. (" + ex.GetType().Name + ")");
            return false;
        }
        finally
        {
            IsBusy = false;
        }
    }
}
