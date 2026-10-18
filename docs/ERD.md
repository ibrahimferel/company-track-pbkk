# ERD CompanyTrack_DB

```mermaid
erDiagram
    Users ||--o{ Pengajuan : "menjadi PIC"
    Users ||--o{ Interaksi : "mencatat"
    Users ||--o{ RiwayatStatus : "mengubah"
    Users ||--o{ Aktivitas : "melakukan"
    Perusahaan ||--o{ Pengajuan : "diajukan"
    PeriodeKunjungan ||--o{ Pengajuan : "berisi"
    AlasanPenolakan |o--o{ Pengajuan : "alasan"
    Pengajuan ||--o{ Interaksi : "memiliki"
    Pengajuan ||--o{ RiwayatStatus : "memiliki"

    Users {
        int UserId PK
        nvarchar Username UK
        nvarchar PasswordHash
        nvarchar Nama
        nvarchar Role "KepalaDepartemen, Staf, BPH"
        bit IsActive
    }
    Perusahaan {
        int PerusahaanId PK
        nvarchar Nama
        nvarchar BidangIndustri
        nvarchar Kota
        nvarchar Email
        nvarchar Telepon
        nvarchar NamaPIC
        nvarchar JabatanPIC
    }
    PeriodeKunjungan {
        int PeriodeId PK
        nvarchar Nama
        date TanggalKunjungan
        int TargetPerusahaan
        bit IsAktif "satu aktif (filtered unique index)"
    }
    AlasanPenolakan {
        int AlasanId PK
        nvarchar Nama UK
    }
    Pengajuan {
        int PengajuanId PK
        int PerusahaanId FK
        int PeriodeId FK
        int UserId FK "PIC internal"
        nvarchar Status "CHECK daftar status"
        date TanggalUsulan
        int KuotaPeserta
        date TanggalKirimSurat
        datetime2 TanggalAktivitasTerakhir
        int AlasanPenolakanId FK "wajib bila Ditolak atau Dibatalkan"
    }
    Interaksi {
        int InteraksiId PK
        int PengajuanId FK
        int UserId FK
        datetime2 Tanggal
        nvarchar Media
        nvarchar Ringkasan
        date TanggalFollowUp
        bit IsSelesai
    }
    RiwayatStatus {
        int RiwayatId PK
        int PengajuanId FK
        nvarchar StatusLama
        nvarchar StatusBaru
        datetime2 Waktu
        int UserId FK
    }
    Pengaturan {
        nvarchar Kunci PK
        nvarchar Nilai
    }
    Aktivitas {
        int AktivitasId PK
        int UserId FK
        nvarchar JenisAksi
        nvarchar Entitas
        int EntitasId
        nvarchar Keterangan
        datetime2 Waktu
    }
```

## Relasi antar tabel

**Pengajuan** adalah tabel utama. Satu pengajuan berarti satu perusahaan yang diajukan untuk dikunjungi pada satu periode kunjungan, dan ditangani oleh satu orang dari tim internal (PIC).

- **Perusahaan dan Pengajuan**: satu perusahaan bisa punya banyak pengajuan, tetapi hanya satu pengajuan di setiap periode. Perusahaan yang sudah punya pengajuan tidak bisa dihapus.
- **Periode Kunjungan dan Pengajuan**: satu periode berisi banyak pengajuan. Hanya ada satu periode yang aktif pada satu waktu.
- **Users dan Pengajuan**: satu pengguna bisa menjadi PIC untuk banyak pengajuan.
- **Alasan Penolakan dan Pengajuan**: satu alasan bisa dipakai banyak pengajuan. Alasan hanya diisi, dan wajib diisi, kalau pengajuan berstatus Ditolak atau Dibatalkan.
- **Pengajuan dan Interaksi**: satu pengajuan punya banyak catatan komunikasi dengan perusahaan, dan setiap catatan dicatat oleh satu pengguna. Kalau pengajuan dihapus, catatannya ikut terhapus.
- **Pengajuan dan Riwayat Status**: setiap perubahan status pengajuan disimpan sebagai riwayat, lengkap dengan siapa yang mengubahnya. Kalau pengajuan dihapus, riwayatnya ikut terhapus.
- **Users dan Aktivitas**: setiap aksi pengguna di sistem tercatat sebagai aktivitas.
- **Pengaturan**: berdiri sendiri, hanya menyimpan pengaturan aplikasi dan tidak terhubung ke tabel lain.

Selain itu, status pengajuan dan media komunikasi hanya boleh berisi pilihan yang sudah ditentukan.
