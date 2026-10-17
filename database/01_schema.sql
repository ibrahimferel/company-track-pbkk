SET QUOTED_IDENTIFIER ON;
GO
/* =========================================================================
   CompanyTrack_DB : skema database (SQL Server)
   Jalankan dengan sqlcmd atau lewat database/setup.sh
   ========================================================================= */
IF DB_ID(N'CompanyTrack_DB') IS NULL
    CREATE DATABASE CompanyTrack_DB;
GO

USE CompanyTrack_DB;
GO

/* Hapus objek lama agar skrip dapat dijalankan ulang (hanya untuk pengembangan) */
IF OBJECT_ID(N'dbo.Aktivitas', N'U') IS NOT NULL DROP TABLE dbo.Aktivitas;
IF OBJECT_ID(N'dbo.RiwayatStatus', N'U') IS NOT NULL DROP TABLE dbo.RiwayatStatus;
IF OBJECT_ID(N'dbo.Interaksi', N'U') IS NOT NULL DROP TABLE dbo.Interaksi;
IF OBJECT_ID(N'dbo.Pengajuan', N'U') IS NOT NULL DROP TABLE dbo.Pengajuan;
IF OBJECT_ID(N'dbo.AlasanPenolakan', N'U') IS NOT NULL DROP TABLE dbo.AlasanPenolakan;
IF OBJECT_ID(N'dbo.PeriodeKunjungan', N'U') IS NOT NULL DROP TABLE dbo.PeriodeKunjungan;
IF OBJECT_ID(N'dbo.Perusahaan', N'U') IS NOT NULL DROP TABLE dbo.Perusahaan;
IF OBJECT_ID(N'dbo.Pengaturan', N'U') IS NOT NULL DROP TABLE dbo.Pengaturan;
IF OBJECT_ID(N'dbo.Users', N'U') IS NOT NULL DROP TABLE dbo.Users;
GO

CREATE TABLE dbo.Users (
    UserId       INT IDENTITY(1,1) CONSTRAINT PK_Users PRIMARY KEY,
    Username     NVARCHAR(50)  NOT NULL CONSTRAINT UQ_Users_Username UNIQUE,
    PasswordHash NVARCHAR(200) NOT NULL,
    Nama         NVARCHAR(100) NOT NULL,
    Role         NVARCHAR(30)  NOT NULL
        CONSTRAINT CK_Users_Role CHECK (Role IN (N'KepalaDepartemen', N'Staf', N'BPH')),
    IsActive     BIT NOT NULL CONSTRAINT DF_Users_IsActive DEFAULT (1)
);
GO

CREATE TABLE dbo.Perusahaan (
    PerusahaanId   INT IDENTITY(1,1) CONSTRAINT PK_Perusahaan PRIMARY KEY,
    Nama           NVARCHAR(150) NOT NULL CONSTRAINT UQ_Perusahaan_Nama UNIQUE,
    BidangIndustri NVARCHAR(80)  NOT NULL,
    Kota           NVARCHAR(80)  NULL,
    Email          NVARCHAR(150) NULL,
    Telepon        NVARCHAR(30)  NULL,
    NamaPIC        NVARCHAR(100) NULL,
    JabatanPIC     NVARCHAR(80)  NULL,
    CreatedAt      DATETIME2(0)  NOT NULL CONSTRAINT DF_Perusahaan_CreatedAt DEFAULT (SYSDATETIME())
);
GO

CREATE TABLE dbo.PeriodeKunjungan (
    PeriodeId        INT IDENTITY(1,1) CONSTRAINT PK_Periode PRIMARY KEY,
    Nama             NVARCHAR(100) NOT NULL,
    TanggalKunjungan DATE NOT NULL,
    TargetPerusahaan INT  NOT NULL CONSTRAINT CK_Periode_Target CHECK (TargetPerusahaan > 0),
    IsAktif          BIT  NOT NULL CONSTRAINT DF_Periode_IsAktif DEFAULT (0)
);
GO

/* Hanya satu periode yang boleh aktif pada satu waktu */
CREATE UNIQUE INDEX UX_Periode_Aktif ON dbo.PeriodeKunjungan (IsAktif) WHERE IsAktif = 1;
GO

CREATE TABLE dbo.AlasanPenolakan (
    AlasanId INT IDENTITY(1,1) CONSTRAINT PK_Alasan PRIMARY KEY,
    Nama     NVARCHAR(100) NOT NULL CONSTRAINT UQ_Alasan_Nama UNIQUE
);
GO

CREATE TABLE dbo.Pengajuan (
    PengajuanId              INT IDENTITY(1,1) CONSTRAINT PK_Pengajuan PRIMARY KEY,
    PerusahaanId             INT NOT NULL CONSTRAINT FK_Pengajuan_Perusahaan REFERENCES dbo.Perusahaan (PerusahaanId),
    PeriodeId                INT NOT NULL CONSTRAINT FK_Pengajuan_Periode    REFERENCES dbo.PeriodeKunjungan (PeriodeId),
    UserId                   INT NOT NULL CONSTRAINT FK_Pengajuan_PIC        REFERENCES dbo.Users (UserId),
    Status                   NVARCHAR(30) NOT NULL CONSTRAINT DF_Pengajuan_Status DEFAULT (N'Target'),
    TanggalUsulan            DATE NULL,
    KuotaPeserta             INT  NULL CONSTRAINT CK_Pengajuan_Kuota CHECK (KuotaPeserta IS NULL OR KuotaPeserta > 0),
    TanggalKirimSurat        DATE NULL,
    TanggalAktivitasTerakhir DATETIME2(0) NOT NULL CONSTRAINT DF_Pengajuan_Aktivitas DEFAULT (SYSDATETIME()),
    AlasanPenolakanId        INT NULL CONSTRAINT FK_Pengajuan_Alasan REFERENCES dbo.AlasanPenolakan (AlasanId),
    CONSTRAINT UQ_Pengajuan_Perusahaan_Periode UNIQUE (PerusahaanId, PeriodeId),
    CONSTRAINT CK_Pengajuan_Status CHECK (Status IN (
        N'Target', N'Dihubungi', N'SuratTerkirim', N'MenungguBalasan',
        N'NegosiasiJadwal', N'Dikonfirmasi', N'Terlaksana', N'Ditolak', N'Dibatalkan')),
    CONSTRAINT CK_Pengajuan_Alasan CHECK (Status NOT IN (N'Ditolak', N'Dibatalkan') OR AlasanPenolakanId IS NOT NULL)
);
GO

CREATE INDEX IX_Pengajuan_Periode_Status ON dbo.Pengajuan (PeriodeId, Status);
CREATE INDEX IX_Pengajuan_PIC ON dbo.Pengajuan (UserId);
GO

CREATE TABLE dbo.Interaksi (
    InteraksiId     INT IDENTITY(1,1) CONSTRAINT PK_Interaksi PRIMARY KEY,
    PengajuanId     INT NOT NULL CONSTRAINT FK_Interaksi_Pengajuan REFERENCES dbo.Pengajuan (PengajuanId) ON DELETE CASCADE,
    UserId          INT NOT NULL CONSTRAINT FK_Interaksi_User REFERENCES dbo.Users (UserId),
    Tanggal         DATETIME2(0) NOT NULL CONSTRAINT DF_Interaksi_Tanggal DEFAULT (SYSDATETIME()),
    Media           NVARCHAR(30) NOT NULL
        CONSTRAINT CK_Interaksi_Media CHECK (Media IN (N'Email', N'WhatsApp', N'Telepon', N'Surat', N'Tatap Muka', N'Lainnya')),
    Ringkasan       NVARCHAR(1000) NOT NULL,
    TanggalFollowUp DATE NULL,
    IsSelesai       BIT NOT NULL CONSTRAINT DF_Interaksi_IsSelesai DEFAULT (0)
);
GO

CREATE INDEX IX_Interaksi_FollowUp ON dbo.Interaksi (TanggalFollowUp) INCLUDE (IsSelesai, PengajuanId);
CREATE INDEX IX_Interaksi_Pengajuan ON dbo.Interaksi (PengajuanId, Tanggal DESC);
GO

CREATE TABLE dbo.RiwayatStatus (
    RiwayatId   INT IDENTITY(1,1) CONSTRAINT PK_Riwayat PRIMARY KEY,
    PengajuanId INT NOT NULL CONSTRAINT FK_Riwayat_Pengajuan REFERENCES dbo.Pengajuan (PengajuanId) ON DELETE CASCADE,
    StatusLama  NVARCHAR(30) NULL,
    StatusBaru  NVARCHAR(30) NOT NULL,
    Waktu       DATETIME2(0) NOT NULL CONSTRAINT DF_Riwayat_Waktu DEFAULT (SYSDATETIME()),
    UserId      INT NOT NULL CONSTRAINT FK_Riwayat_User REFERENCES dbo.Users (UserId)
);
GO

CREATE INDEX IX_Riwayat_Pengajuan ON dbo.RiwayatStatus (PengajuanId, Waktu DESC);
GO

CREATE TABLE dbo.Pengaturan (
    Kunci NVARCHAR(50)  NOT NULL CONSTRAINT PK_Pengaturan PRIMARY KEY,
    Nilai NVARCHAR(200) NOT NULL
);
GO

/* Aktivitas: jangan menyimpan password atau data rahasia pada kolom Keterangan */
CREATE TABLE dbo.Aktivitas (
    AktivitasId INT IDENTITY(1,1) CONSTRAINT PK_Aktivitas PRIMARY KEY,
    UserId      INT NOT NULL CONSTRAINT FK_Aktivitas_User REFERENCES dbo.Users (UserId),
    JenisAksi   NVARCHAR(30)  NOT NULL,
    Entitas     NVARCHAR(50)  NOT NULL,
    EntitasId   INT NULL,
    Keterangan  NVARCHAR(200) NOT NULL,
    Waktu       DATETIME2(0)  NOT NULL CONSTRAINT DF_Aktivitas_Waktu DEFAULT (SYSDATETIME())
);
GO

CREATE INDEX IX_Aktivitas_Waktu ON dbo.Aktivitas (Waktu DESC);
GO

PRINT N'Skema CompanyTrack_DB selesai dibuat.';
GO