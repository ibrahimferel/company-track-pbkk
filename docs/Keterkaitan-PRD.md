# Keterkaitan PRD dengan implementasi

Tabel ini memetakan kemampuan di PRD ke tempatnya dalam kode, agar dosen dan tim mudah memeriksa kelengkapan.
Nomor FR/US mengikuti PRD. Kolom status jujur: **Ada** berarti sudah ditulis, **Belum** berarti sengaja tidak dikerjakan.

| Kemampuan | Lokasi utama | Status |
|---|---|---|
| Login dan verifikasi password berhash | `AuthService`, `PasswordHasher`, `LoginView` | Ada |
| Peran dan hak akses (Kepala Departemen, Staf, BPH) | `AccessPolicy`, `Session`, pemeriksaan di setiap service | Ada |
| CRUD perusahaan dengan validasi | `MasterService`, `MasterRepository`, `Validator`, `PerusahaanView` | Ada |
| Kelola periode kunjungan dan target | `MasterService.SavePeriodeAsync`, `PeriodeView` | Ada |
| Buat, ubah, hapus pengajuan (satu per perusahaan per periode) | `PengajuanService`, `PengajuanListView` | Ada |
| Cari dan filter pengajuan | `PengajuanRepository.QueryAsync`, `PengajuanListViewModel` | Ada |
| Pipeline status dengan aturan transisi | `StatusRules`, `PengajuanService.ChangeStatusAsync` | Ada |
| Alasan wajib untuk Ditolak dan Dibatalkan | `StatusRules.EnsureTransition`, CHECK database | Ada |
| Riwayat status | tabel `RiwayatStatus`, `PengajuanDetailView` | Ada |
| Catat komunikasi dan jadwalkan follow-up | `PengajuanService.AddInteraksiAsync`, `PengajuanDetailView` | Ada |
| Halaman follow-up (terlambat, hari ini, minggu ini) | `FollowUpClassifier`, `FollowUpView` | Ada |
| Deteksi balasan tertunda dengan ambang yang dapat diatur | `StaleChecker`, `PengaturanView` | Ada |
| Dashboard (kartu, hitung mundur, prioritas, funnel, aktivitas) | `DashboardService`, `DashboardView` | Ada |
| Laporan funnel, per industri, alasan penolakan | `LaporanViewModel`, `PengajuanRepository` | Ada |
| Log aktivitas | `MasterRepository.LogAsync`, dipanggil dalam transaksi | Ada |
| Tema gelap dan terang | `ThemeService`, `App.axaml` | Ada |
| Notifikasi hasil aksi dan konfirmasi hapus inline | `Notifier`, baris dengan `IsConfirmingDelete` | Ada |
| Manajemen pengguna lewat UI | - | Belum |
| Evaluasi pasca kunjungan | - | Belum |
| Ekspor laporan | - | Belum |

## Pengujian

| Area | Berkas tes |
|---|---|
| Transisi dan definisi aktif | `StatusRulesTests` |
| Pengelompokan follow-up | `FollowUpClassifierTests` |
| Balasan tertunda | `StaleCheckerTests` |
| Hash password dan akun seed | `PasswordHasherTests` |
| Validasi dan matriks hak akses | `ValidatorAndAccessTests` |

Lapisan repository dan UI diuji manual dengan skenario pada `docs/Naskah-Video.md` (setiap adegan sekaligus berfungsi sebagai skenario uji).
