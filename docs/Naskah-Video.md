# Naskah video demo CompanyTrack

Perkiraan durasi 8 sampai 10 menit. Rekam layar dengan QuickTime (Cmd+Shift+5). Bagi peran bicara antar tiga anggota agar semua terlibat.
Setiap adegan juga menjadi skenario uji manual: bila hasil yang diharapkan tidak muncul, catat sebagai bug.

Sebelum merekam: jalankan `database/setup.sh` agar data kembali ke kondisi awal, lalu `dotnet run --project src/CompanyTrack`.

## 1. Pembukaan (0:00 - 1:00) Anggota 1
- Perkenalkan kelompok, mata kuliah, dan dosen.
- Masalah: pipeline Company Visit ke banyak perusahaan sulit dipantau sehingga follow-up terlewat dan balasan tertunda tidak terlihat.
- Solusi: CompanyTrack, aplikasi desktop Avalonia UI dengan SQL Server.

## 2. Teknologi (1:00 - 1:45) Anggota 1
- Tunjukkan struktur solusi: Core (aturan bisnis), aplikasi Avalonia (MVVM), tes, database.
- Tunjukkan Docker Desktop dengan container `companytrack-sql` berjalan.

## 3. Login dan peran (1:45 - 3:00) Anggota 2
1. Coba login dengan password salah. Hasil: pesan "Username atau password salah."
2. Login sebagai `staf` / `staf123`.
3. Tunjukkan sidebar, badge Follow-up, dan tombol tema gelap/terang.

## 4. Dashboard (3:00 - 4:15) Anggota 2
- Kartu: follow-up hari ini, terlambat, balasan tertunda, terkonfirmasi vs target.
- Hitung mundur menuju hari kunjungan.
- Daftar "Perlu perhatian" dan funnel. Klik **Buka** pada satu perusahaan.

## 5. Pengajuan dan komunikasi (4:15 - 6:30) Anggota 3
1. Menu Pengajuan: tunjukkan pencarian dan filter status/PIC.
2. Tambah pengajuan baru. Coba tambah perusahaan yang sama pada periode yang sama. Hasil: ditolak dengan pesan jelas.
3. Buka detail pengajuan. Catat komunikasi dengan tanggal follow-up kemarin. Hasil: ditolak (tanggal masa lalu). Ulangi dengan tanggal besok. Hasil: tersimpan dan tampil di riwayat.
4. Ubah status Target ke Dihubungi, lalu ke Surat Terkirim. Tunjukkan riwayat status.
5. Pilih status Ditolak tanpa alasan. Hasil: ditolak. Pilih alasan lalu terapkan. Hasil: berhasil, status menjadi final.

## 6. Follow-up (6:30 - 7:15) Anggota 2
- Tab Terlambat, Hari ini, Minggu ini. Tandai satu follow-up selesai. Hasil: badge berkurang.

## 7. Hak akses (7:15 - 8:15) Anggota 3
1. Keluar, login sebagai `bph` / `bph123`. Hasil: tombol tambah/ubah/hapus tidak tampil, data tetap bisa dilihat.
2. Keluar, login sebagai `kadep` / `kadep123`. Buka Periode dan Pengaturan, ubah ambang balasan tertunda dari 7 ke 5 hari. Kembali ke Dashboard. Hasil: jumlah balasan tertunda bertambah.

## 8. Laporan dan tema (8:15 - 9:00) Anggota 1
- Laporan: funnel, tingkat keberhasilan per industri, alasan penolakan.
- Ganti ke mode terang, tunjukkan tampilan tetap terbaca.

## 9. Penutup (9:00 - 9:45) Anggota 1
- Ringkas fitur dan teknologi. Sebutkan keterbatasan: manajemen pengguna lewat UI, evaluasi pasca kunjungan, dan ekspor laporan belum ada.
- Terima kasih.
