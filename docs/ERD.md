# ERD CompanyTrack_DB

Diagram ini ditulis dalam Mermaid. Pratinjau dapat dilihat di GitHub, VS Code (ekstensi Markdown Preview Mermaid), atau https://mermaid.live.

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

## Aturan integritas

| Aturan | Cara penegakan |
|---|---|
| Satu perusahaan hanya sekali per periode | `UNIQUE (PerusahaanId, PeriodeId)` pada Pengajuan |
| Hanya satu periode aktif | Filtered unique index `UX_Periode_Aktif` pada `IsAktif = 1` |
| Status hanya nilai yang dikenal | `CHECK` pada Pengajuan.Status |
| Ditolak dan Dibatalkan wajib beralasan | `CHECK CK_Pengajuan_Alasan` |
| Media komunikasi terbatas | `CHECK` pada Interaksi.Media |
| Hapus pengajuan ikut menghapus interaksi dan riwayat | `ON DELETE CASCADE` |
| Perusahaan yang punya pengajuan tidak bisa dihapus | FK tanpa cascade, dicek juga di service |

Follow-up terlambat dan balasan tertunda tidak punya kolom. Keduanya dihitung dari `Interaksi.TanggalFollowUp`, `Interaksi.IsSelesai`, dan `Pengajuan.TanggalAktivitasTerakhir`.
