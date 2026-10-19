# CompanyTrack

Aplikasi desktop (Rich UI) untuk memantau pipeline dan follow-up proker **Company Visit** Departemen External Affairs HMTC ITS.
Tugas Pemrograman Berbasis Kerangka Kerja (PBKK C) 2026, dibuat dengan Avalonia UI, C#, .NET 8, dan SQLite.

## Tech Stack

| Bagian             | Pilihan                                                       |
| ------------------ | ------------------------------------------------------------- |
| UI                 | Avalonia UI 11 (XAML), pola MVVM dengan CommunityToolkit.Mvvm |
| Bahasa dan runtime | C# 12, .NET 8                                                 |
| Database           | SQLite (satu file, dibuat otomatis, tanpa server)             |
| Akses data         | Microsoft.Data.Sqlite, query berparameter, transaksi          |
| Tes                | xUnit (lapisan Core)                                          |

## Project Structure

```
CompanyTrack.sln
src/CompanyTrack.Core/    model, aturan bisnis, keamanan, repository, service (tanpa UI)
src/CompanyTrack.Core/Sql skema dan data awal SQLite (embedded resource)
src/CompanyTrack/         aplikasi Avalonia (ViewModels, Views, tema)
tests/CompanyTrack.Tests/ tes unit xUnit
docs/                     ERD, keterkaitan PRD, naskah video
```

## Prerequisites

[.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (untuk Mac Apple Silicon pilih Arm64). Tidak perlu Docker atau SQL Server.

## Langkah menjalankan

```bash
dotnet build
dotnet test
dotnet run --project src/CompanyTrack
```

Saat pertama dijalankan, aplikasi membuat file database `companytrack.db` berisi skema dan data contoh di:

- Windows: `%APPDATA%\CompanyTrack\companytrack.db`
- macOS: `~/Library/Application Support/CompanyTrack/companytrack.db`

Untuk mengulang dari data awal, tutup aplikasi lalu hapus file itu. Lokasi lain dapat dipilih dengan variabel lingkungan `COMPANYTRACK_DB` (path file).
Data tersimpan lokal di tiap komputer, tidak dibagikan antar pengguna.

## Membuat paket untuk dibagikan

```bash
./publish.sh
```

Hasilnya di `dist/`:

| File                          | Untuk                                                       |
| ----------------------------- | ----------------------------------------------------------- |
| `CompanyTrack.exe`            | Windows 64-bit, satu file, tidak perlu menginstal .NET      |
| `CompanyTrack-mac-arm64.zip`  | Mac Apple Silicon (M1 dan seterusnya), berisi CompanyTrack.app |
| `CompanyTrack-mac-x64.zip`    | Mac Intel                                                   |

Aplikasi belum ditandatangani resmi. Windows menampilkan peringatan SmartScreen: pilih _More info_ lalu _Run anyway_.
Di Mac, ekstrak zip, lalu pada pembukaan pertama klik kanan `CompanyTrack.app` dan pilih _Open_ (atau _System Settings, Privacy and Security, Open Anyway_).

## Akun demo

| Username | Password | Peran             | Kemampuan                                           |
| -------- | -------- | ----------------- | --------------------------------------------------- |
| kadep    | kadep123 | Kepala Departemen | Semua fitur, termasuk periode dan pengaturan        |
| staf     | staf123  | Staf              | Kelola perusahaan, pengajuan, komunikasi, follow-up |
| staf2    | staf123  | Staf              | Sama dengan staf                                    |
| bph      | bph123   | BPH               | Hanya baca                                          |

## Aturan penting

- Alur status: Target, Dihubungi, Surat Terkirim, Menunggu Balasan, Negosiasi Jadwal, Dikonfirmasi, Terlaksana. Ditolak dan Dibatalkan wajib disertai alasan. Perpindahan yang tidak sah ditolak oleh `StatusRules`.
- Perubahan status dan pencatatan komunikasi berjalan dalam satu transaksi (update pengajuan, riwayat, log aktivitas).
- Follow-up terlambat dan balasan tertunda dihitung dari data saat ini, tidak disimpan. Ambang balasan tertunda (bawaan 7 hari) dapat diubah Kepala Departemen di menu Pengaturan.
- Hak akses ditegakkan di lapisan service, bukan hanya dengan menyembunyikan tombol.
- Password disimpan sebagai hash PBKDF2-SHA256 (100.000 iterasi, salt acak).
