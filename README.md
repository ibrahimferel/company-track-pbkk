# CompanyTrack

Aplikasi desktop (Rich UI) untuk memantau pipeline dan follow-up proker **Company Visit** Departemen External Affairs HMTC ITS.
Tugas Pemrograman Berbasis Kerangka Kerja (PBKK), dibuat dengan Avalonia UI, C#, .NET 8, dan SQL Server.

## Teknologi

| Bagian | Pilihan |
|---|---|
| UI | Avalonia UI 11 (XAML), pola MVVM dengan CommunityToolkit.Mvvm |
| Bahasa dan runtime | C# 12, .NET 8 |
| Database | SQL Server 2022 (Docker) |
| Akses data | Microsoft.Data.SqlClient, query berparameter, transaksi |
| Tes | xUnit (lapisan Core) |

## Struktur

```
CompanyTrack.sln
database/                 skema, data awal, login aplikasi, setup.sh
src/CompanyTrack.Core/    model, aturan bisnis, keamanan, repository, service (tanpa UI)
src/CompanyTrack/         aplikasi Avalonia (ViewModels, Views, tema)
tests/CompanyTrack.Tests/ tes unit xUnit
docs/                     ERD, keterkaitan PRD, naskah video
```

## Prasyarat (Mac)

1. [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (untuk Mac Apple Silicon pilih Arm64).
2. Docker Desktop. Pada Mac Apple Silicon aktifkan *Use Rosetta for x86_64/amd64 emulation* di Settings, General.

## Langkah menjalankan

```bash
# 1. Jalankan SQL Server dan buat database beserta data contoh
cd database
./setup.sh

# 2. Build dan tes
cd ..
dotnet build
dotnet test

# 3. Jalankan aplikasi
dotnet run --project src/CompanyTrack
```

`setup.sh` membuat container `companytrack-sql`, menjalankan `01_schema.sql`, `02_seed.sql`, `03_app_login.sql`, lalu mencetak connection string.
Nilai bawaan sudah cocok dengan `src/CompanyTrack/appsettings.json`. Bila password diganti, ubah `appsettings.json` atau set variabel lingkungan:

```bash
export COMPANYTRACK_CONNECTION="Server=localhost,1433;Database=CompanyTrack_DB;User Id=ct_app;Password=...;TrustServerCertificate=True;Encrypt=True"
```

Untuk mengulang dari data awal, jalankan lagi `./setup.sh`. Container dipakai ulang, tabel dihapus lalu dibuat ulang beserta data contoh.

## Akun demo

Semua akun hanya untuk pengembangan. Jangan dipakai di lingkungan nyata.

| Username | Password | Peran | Kemampuan |
|---|---|---|---|
| kadep | kadep123 | Kepala Departemen | Semua fitur, termasuk periode dan pengaturan |
| staf | staf123 | Staf | Kelola perusahaan, pengajuan, komunikasi, follow-up |
| staf2 | staf123 | Staf | Sama dengan staf |
| bph | bph123 | BPH | Hanya baca |

## Aturan penting

- Alur status: Target, Dihubungi, Surat Terkirim, Menunggu Balasan, Negosiasi Jadwal, Dikonfirmasi, Terlaksana. Ditolak dan Dibatalkan wajib disertai alasan. Perpindahan yang tidak sah ditolak oleh `StatusRules`.
- Perubahan status dan pencatatan komunikasi berjalan dalam satu transaksi (update pengajuan, riwayat, log aktivitas).
- Follow-up terlambat dan balasan tertunda dihitung dari data saat ini, tidak disimpan. Ambang balasan tertunda (bawaan 7 hari) dapat diubah Kepala Departemen di menu Pengaturan.
- Hak akses ditegakkan di lapisan service, bukan hanya dengan menyembunyikan tombol.
- Password disimpan sebagai hash PBKDF2-SHA256 (100.000 iterasi, salt acak).

## Batasan yang diketahui

- Manajemen pengguna lewat UI belum ada. Akun ditambah lewat data awal SQL.
- Evaluasi pasca kunjungan dan ekspor laporan belum diimplementasikan.
- Aplikasi memakai satu koneksi database lokal. Tidak ada mode offline.
