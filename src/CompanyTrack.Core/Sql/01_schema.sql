/* =========================================================================
   CompanyTrack : skema database (SQLite)
   Dijalankan otomatis oleh Db saat file database belum ada.
   Tanggal: TEXT 'yyyy-MM-dd'. Waktu: TEXT 'yyyy-MM-dd HH:mm:ss' (waktu lokal).
   Boolean: INTEGER 0/1.
   ========================================================================= */

CREATE TABLE Users (
    UserId       INTEGER PRIMARY KEY AUTOINCREMENT,
    Username     TEXT NOT NULL UNIQUE,
    PasswordHash TEXT NOT NULL,
    Nama         TEXT NOT NULL,
    Role         TEXT NOT NULL CHECK (Role IN ('KepalaDepartemen', 'Staf', 'BPH')),
    IsActive     INTEGER NOT NULL DEFAULT 1
);

CREATE TABLE Perusahaan (
    PerusahaanId   INTEGER PRIMARY KEY AUTOINCREMENT,
    Nama           TEXT NOT NULL UNIQUE,
    BidangIndustri TEXT NOT NULL,
    Kota           TEXT NULL,
    Email          TEXT NULL,
    Telepon        TEXT NULL,
    NamaPIC        TEXT NULL,
    JabatanPIC     TEXT NULL,
    CreatedAt      TEXT NOT NULL DEFAULT (datetime('now', 'localtime'))
);

CREATE TABLE PeriodeKunjungan (
    PeriodeId        INTEGER PRIMARY KEY AUTOINCREMENT,
    Nama             TEXT NOT NULL,
    TanggalKunjungan TEXT NOT NULL,
    TargetPerusahaan INTEGER NOT NULL CHECK (TargetPerusahaan > 0),
    IsAktif          INTEGER NOT NULL DEFAULT 0
);

/* Hanya satu periode yang boleh aktif pada satu waktu */
CREATE UNIQUE INDEX UX_Periode_Aktif ON PeriodeKunjungan (IsAktif) WHERE IsAktif = 1;

CREATE TABLE AlasanPenolakan (
    AlasanId INTEGER PRIMARY KEY AUTOINCREMENT,
    Nama     TEXT NOT NULL UNIQUE
);

CREATE TABLE Pengajuan (
    PengajuanId              INTEGER PRIMARY KEY AUTOINCREMENT,
    PerusahaanId             INTEGER NOT NULL REFERENCES Perusahaan (PerusahaanId),
    PeriodeId                INTEGER NOT NULL REFERENCES PeriodeKunjungan (PeriodeId),
    UserId                   INTEGER NOT NULL REFERENCES Users (UserId),
    Status                   TEXT NOT NULL DEFAULT 'Target',
    TanggalUsulan            TEXT NULL,
    KuotaPeserta             INTEGER NULL CHECK (KuotaPeserta IS NULL OR KuotaPeserta > 0),
    TanggalKirimSurat        TEXT NULL,
    TanggalAktivitasTerakhir TEXT NOT NULL DEFAULT (datetime('now', 'localtime')),
    AlasanPenolakanId        INTEGER NULL REFERENCES AlasanPenolakan (AlasanId),
    CONSTRAINT UQ_Pengajuan_Perusahaan_Periode UNIQUE (PerusahaanId, PeriodeId),
    CONSTRAINT CK_Pengajuan_Status CHECK (Status IN (
        'Target', 'Dihubungi', 'SuratTerkirim', 'MenungguBalasan',
        'NegosiasiJadwal', 'Dikonfirmasi', 'Terlaksana', 'Ditolak', 'Dibatalkan')),
    CONSTRAINT CK_Pengajuan_Alasan CHECK (Status NOT IN ('Ditolak', 'Dibatalkan') OR AlasanPenolakanId IS NOT NULL)
);

CREATE INDEX IX_Pengajuan_Periode_Status ON Pengajuan (PeriodeId, Status);
CREATE INDEX IX_Pengajuan_PIC ON Pengajuan (UserId);

CREATE TABLE Interaksi (
    InteraksiId     INTEGER PRIMARY KEY AUTOINCREMENT,
    PengajuanId     INTEGER NOT NULL REFERENCES Pengajuan (PengajuanId) ON DELETE CASCADE,
    UserId          INTEGER NOT NULL REFERENCES Users (UserId),
    Tanggal         TEXT NOT NULL DEFAULT (datetime('now', 'localtime')),
    Media           TEXT NOT NULL
        CHECK (Media IN ('Email', 'WhatsApp', 'Telepon', 'Surat', 'Tatap Muka', 'Lainnya')),
    Ringkasan       TEXT NOT NULL,
    TanggalFollowUp TEXT NULL,
    IsSelesai       INTEGER NOT NULL DEFAULT 0
);

CREATE INDEX IX_Interaksi_FollowUp ON Interaksi (TanggalFollowUp, IsSelesai, PengajuanId);
CREATE INDEX IX_Interaksi_Pengajuan ON Interaksi (PengajuanId, Tanggal DESC);

CREATE TABLE RiwayatStatus (
    RiwayatId   INTEGER PRIMARY KEY AUTOINCREMENT,
    PengajuanId INTEGER NOT NULL REFERENCES Pengajuan (PengajuanId) ON DELETE CASCADE,
    StatusLama  TEXT NULL,
    StatusBaru  TEXT NOT NULL,
    Waktu       TEXT NOT NULL DEFAULT (datetime('now', 'localtime')),
    UserId      INTEGER NOT NULL REFERENCES Users (UserId)
);

CREATE INDEX IX_Riwayat_Pengajuan ON RiwayatStatus (PengajuanId, Waktu DESC);

CREATE TABLE Pengaturan (
    Kunci TEXT NOT NULL PRIMARY KEY,
    Nilai TEXT NOT NULL
);

/* Aktivitas: jangan menyimpan password atau data rahasia pada kolom Keterangan */
CREATE TABLE Aktivitas (
    AktivitasId INTEGER PRIMARY KEY AUTOINCREMENT,
    UserId      INTEGER NOT NULL REFERENCES Users (UserId),
    JenisAksi   TEXT NOT NULL,
    Entitas     TEXT NOT NULL,
    EntitasId   INTEGER NULL,
    Keterangan  TEXT NOT NULL,
    Waktu       TEXT NOT NULL DEFAULT (datetime('now', 'localtime'))
);

CREATE INDEX IX_Aktivitas_Waktu ON Aktivitas (Waktu DESC);
