SET QUOTED_IDENTIFIER ON;
GO
/* =========================================================================
   CompanyTrack_DB : data seed DUMMY untuk pengembangan, pengujian, dan demo
   Semua nama perusahaan dan akun adalah fiktif. Tanggal dihitung relatif
   terhadap hari ini supaya dashboard selalu punya data terlambat/hari ini.

   Akun demo (ubah sebelum dipakai sungguhan):
     kadep / kadep123   : KepalaDepartemen
     staf  / staf123    : Staf
     staf2 / staf123    : Staf
     bph   / bph123     : BPH (hanya baca)
   ========================================================================= */
USE CompanyTrack_DB;
GO

SET NOCOUNT ON;

/* ---------- Users (hash PBKDF2-SHA256, format PBKDF2$iterasi$salt$hash) ---------- */
INSERT INTO dbo.Users (Username, PasswordHash, Nama, Role, IsActive) VALUES
(N'kadep', N'PBKDF2$100000$06gjbvQSNdr6ZHHRy7YlUQ==$jidEX1l4vMfebORKh8fhfSbYxARgZ9InYD+v9p9CTjo=', N'Kepala Departemen External Affairs', N'KepalaDepartemen', 1),
(N'staf',  N'PBKDF2$100000$NreTIN7fgRkpMr/OjfZYRw==$0olrMRnChV2PSSI0DpplSST66dB4GGYepfegxxW2qVk=', N'Staf External Affairs 1', N'Staf', 1),
(N'staf2', N'PBKDF2$100000$NreTIN7fgRkpMr/OjfZYRw==$0olrMRnChV2PSSI0DpplSST66dB4GGYepfegxxW2qVk=', N'Staf External Affairs 2', N'Staf', 1),
(N'bph',   N'PBKDF2$100000$LomHhAbju+suxKn2CpB7FQ==$2q76rRZuoGaw7HXcs7QEmiiyx1wJkieGN/GwTF2qvio=', N'Perwakilan BPH', N'BPH', 1);

/* ---------- Master ---------- */
INSERT INTO dbo.AlasanPenolakan (Nama) VALUES
(N'Jadwal perusahaan penuh'),
(N'Kebijakan perusahaan tidak menerima kunjungan'),
(N'Kuota peserta tidak mencukupi'),
(N'Tidak ada respons'),
(N'Lainnya');

INSERT INTO dbo.Pengaturan (Kunci, Nilai) VALUES (N'AmbangBalasanHari', N'7');

INSERT INTO dbo.PeriodeKunjungan (Nama, TanggalKunjungan, TargetPerusahaan, IsAktif) VALUES
(N'Company Visit 2025', DATEADD(DAY, -300, CAST(SYSDATETIME() AS DATE)), 4, 0),
(N'Company Visit 2026', DATEADD(DAY, 55,   CAST(SYSDATETIME() AS DATE)), 5, 1);

INSERT INTO dbo.Perusahaan (Nama, BidangIndustri, Kota, Email, Telepon, NamaPIC, JabatanPIC) VALUES
(N'PT Nusantara Digital Solusi',   N'Teknologi Informasi', N'Jakarta',    N'hr@nusantara-digital.example', N'021-5550101', N'Rina Wulandari',  N'HR Business Partner'),
(N'PT Arunika Data Teknologi',     N'Data dan AI',         N'Surabaya',   N'talent@arunika.example',       N'031-5550102', N'Dimas Prakoso',   N'Talent Acquisition'),
(N'PT Cakrawala Cloud Indonesia',  N'Cloud Computing',     N'Bandung',    N'people@cakrawala.example',     N'022-5550103', N'Maya Sari',       N'People Operations'),
(N'PT Bahari Fintek Mandiri',      N'Fintech',             N'Jakarta',    N'campus@bahari-fintek.example', N'021-5550104', N'Anton Gunawan',   N'Campus Relations'),
(N'PT Dwipa Game Studio',          N'Game Development',    N'Yogyakarta', N'studio@dwipa.example',         N'0274-5550105', N'Putri Lestari',  N'Studio Manager'),
(N'PT Eka Siber Aman',             N'Keamanan Siber',      N'Jakarta',    N'hr@ekasiber.example',          N'021-5550106', N'Fajar Nugroho',   N'HR Manager'),
(N'PT Garuda Logistik Cerdas',     N'E-Commerce dan Logistik', N'Surabaya', N'csr@garuda-logistik.example', N'031-5550107', N'Hendra Wijaya',   N'CSR Officer'),
(N'PT Harapan Telekomunikasi Nusantara', N'Telekomunikasi', N'Jakarta',   N'edu@harapan-telko.example',    N'021-5550108', N'Sinta Maharani',  N'Education Program'),
(N'PT Indra Perangkat Lunak',      N'Software House',      N'Malang',     N'hello@indra-pl.example',       N'0341-5550109', N'Bayu Setiawan',   N'Operations Lead'),
(N'PT Jaya Manufaktur Otomasi',    N'Manufaktur dan IoT',  N'Sidoarjo',   N'hr@jaya-otomasi.example',      N'031-5550110', N'Lina Marlina',    N'HR Supervisor'),
(N'PT Kirana Media Kreatif',       N'Media Digital',       N'Surabaya',   N'info@kirana-media.example',    N'031-5550111', N'Rudi Hartono',    N'Brand Manager'),
(N'PT Lintas Startup Raya',        N'Startup',             N'Denpasar',   N'team@lintas-startup.example',  N'0361-5550112', N'Nadia Pratiwi',   N'Co-Founder');
GO

/* ---------- Pengajuan periode 2026 (aktif) dan 2025 (riwayat) ---------- */
DECLARE @pAktif INT = (SELECT PeriodeId FROM dbo.PeriodeKunjungan WHERE IsAktif = 1);
DECLARE @pLama  INT = (SELECT PeriodeId FROM dbo.PeriodeKunjungan WHERE IsAktif = 0);
DECLARE @today  DATE = CAST(SYSDATETIME() AS DATE);

DECLARE @s TABLE (
    Perusahaan NVARCHAR(150), Pic NVARCHAR(50), Status NVARCHAR(30),
    UsulOffset INT NULL, Kuota INT NULL, KirimOffset INT NULL, AktivitasOffset INT NOT NULL, Alasan NVARCHAR(100) NULL);

INSERT INTO @s VALUES
(N'PT Nusantara Digital Solusi',          N'staf',  N'Dikonfirmasi',     55, 30, -30,  -5, NULL),
(N'PT Arunika Data Teknologi',            N'staf',  N'Dikonfirmasi',     56, 25, -28,  -4, NULL),
(N'PT Jaya Manufaktur Otomasi',           N'staf2', N'Dikonfirmasi',     57, 20, -26,  -6, NULL),
(N'PT Cakrawala Cloud Indonesia',         N'staf2', N'NegosiasiJadwal',  58, 30, -20,  -3, NULL),
(N'PT Bahari Fintek Mandiri',             N'staf',  N'MenungguBalasan',  60, 20, -18, -12, NULL),
(N'PT Garuda Logistik Cerdas',            N'staf2', N'MenungguBalasan',  62, 40, -14,  -9, NULL),
(N'PT Dwipa Game Studio',                 N'staf2', N'SuratTerkirim',  NULL, NULL, -9,  -9, NULL),
(N'PT Eka Siber Aman',                    N'staf',  N'Dihubungi',      NULL, NULL, NULL, -2, NULL),
(N'PT Harapan Telekomunikasi Nusantara',  N'kadep', N'Target',         NULL, NULL, NULL, -1, NULL),
(N'PT Indra Perangkat Lunak',             N'staf',  N'Ditolak',        NULL, NULL, -25, -15, N'Jadwal perusahaan penuh'),
(N'PT Kirana Media Kreatif',              N'staf',  N'Dibatalkan',     NULL, NULL, -22, -10, N'Kuota peserta tidak mencukupi');

INSERT INTO dbo.Pengajuan (PerusahaanId, PeriodeId, UserId, Status, TanggalUsulan, KuotaPeserta,
                           TanggalKirimSurat, TanggalAktivitasTerakhir, AlasanPenolakanId)
SELECT c.PerusahaanId, @pAktif, u.UserId, s.Status,
       CASE WHEN s.UsulOffset  IS NULL THEN NULL ELSE DATEADD(DAY, s.UsulOffset,  @today) END,
       s.Kuota,
       CASE WHEN s.KirimOffset IS NULL THEN NULL ELSE DATEADD(DAY, s.KirimOffset, @today) END,
       DATEADD(DAY, s.AktivitasOffset, SYSDATETIME()),
       a.AlasanId
FROM @s s
JOIN dbo.Perusahaan c ON c.Nama = s.Perusahaan
JOIN dbo.Users u ON u.Username = s.Pic
LEFT JOIN dbo.AlasanPenolakan a ON a.Nama = s.Alasan;

DECLARE @h TABLE (
    Perusahaan NVARCHAR(150), Pic NVARCHAR(50), Status NVARCHAR(30),
    UsulOffset INT NULL, Kuota INT NULL, AktivitasOffset INT NOT NULL, Alasan NVARCHAR(100) NULL);

INSERT INTO @h VALUES
(N'PT Nusantara Digital Solusi', N'staf',  N'Terlaksana', -300, 25, -310, NULL),
(N'PT Lintas Startup Raya',      N'staf2', N'Terlaksana', -300, 20, -312, NULL),
(N'PT Indra Perangkat Lunak',    N'staf',  N'Ditolak',    NULL, NULL, -340, N'Kebijakan perusahaan tidak menerima kunjungan'),
(N'PT Eka Siber Aman',           N'staf2', N'Ditolak',    NULL, NULL, -335, N'Tidak ada respons');

INSERT INTO dbo.Pengajuan (PerusahaanId, PeriodeId, UserId, Status, TanggalUsulan, KuotaPeserta,
                           TanggalKirimSurat, TanggalAktivitasTerakhir, AlasanPenolakanId)
SELECT c.PerusahaanId, @pLama, u.UserId, h.Status,
       CASE WHEN h.UsulOffset IS NULL THEN NULL ELSE DATEADD(DAY, h.UsulOffset, @today) END,
       h.Kuota, NULL, DATEADD(DAY, h.AktivitasOffset, SYSDATETIME()), a.AlasanId
FROM @h h
JOIN dbo.Perusahaan c ON c.Nama = h.Perusahaan
JOIN dbo.Users u ON u.Username = h.Pic
LEFT JOIN dbo.AlasanPenolakan a ON a.Nama = h.Alasan;

/* Riwayat status: satu baris Target lalu satu baris menuju status terkini */
INSERT INTO dbo.RiwayatStatus (PengajuanId, StatusLama, StatusBaru, Waktu, UserId)
SELECT p.PengajuanId, NULL, N'Target', DATEADD(DAY, -2, p.TanggalAktivitasTerakhir), p.UserId
FROM dbo.Pengajuan p;

INSERT INTO dbo.RiwayatStatus (PengajuanId, StatusLama, StatusBaru, Waktu, UserId)
SELECT p.PengajuanId, N'Target', p.Status, p.TanggalAktivitasTerakhir, p.UserId
FROM dbo.Pengajuan p
WHERE p.Status <> N'Target';
GO

/* ---------- Interaksi dan follow-up ---------- */
DECLARE @pAktif2 INT = (SELECT PeriodeId FROM dbo.PeriodeKunjungan WHERE IsAktif = 1);

DECLARE @i TABLE (
    Perusahaan NVARCHAR(150), Pic NVARCHAR(50), HariOffset INT, Media NVARCHAR(30),
    Ringkasan NVARCHAR(1000), FollowUpOffset INT NULL, Selesai BIT);

INSERT INTO @i VALUES
(N'PT Nusantara Digital Solusi', N'staf',  -30, N'Surat',    N'Mengirim surat resmi permohonan company visit.', NULL, 1),
(N'PT Nusantara Digital Solusi', N'staf',   -5, N'Email',    N'Perusahaan mengonfirmasi tanggal kunjungan.', NULL, 1),
(N'PT Arunika Data Teknologi',   N'staf',  -28, N'Email',    N'Mengirim proposal kunjungan ke tim talent acquisition.', NULL, 1),
(N'PT Arunika Data Teknologi',   N'staf',   -4, N'Telepon',  N'Konfirmasi tanggal dan kuota peserta.', NULL, 1),
(N'PT Cakrawala Cloud Indonesia',N'staf2', -20, N'Email',    N'Mengirim surat resmi permohonan kunjungan.', -13, 1),
(N'PT Cakrawala Cloud Indonesia',N'staf2',  -3, N'WhatsApp', N'Perusahaan menawarkan dua opsi tanggal, menunggu keputusan internal HMTC.', 0, 0),
(N'PT Bahari Fintek Mandiri',    N'staf',  -18, N'Surat',    N'Surat permohonan kunjungan diterima resepsionis.', -12, 1),
(N'PT Bahari Fintek Mandiri',    N'staf',  -12, N'Email',    N'Follow-up pertama, belum ada balasan.', -5, 0),
(N'PT Garuda Logistik Cerdas',   N'staf2', -14, N'Email',    N'Mengirim proposal kunjungan ke CSR Officer.', -7, 1),
(N'PT Garuda Logistik Cerdas',   N'staf2',  -9, N'Telepon',  N'Telepon tidak diangkat, akan dicoba lagi.', 3, 0),
(N'PT Dwipa Game Studio',        N'staf2',  -9, N'Email',    N'Surat terkirim ke studio manager.', -2, 0),
(N'PT Eka Siber Aman',           N'staf',   -2, N'Telepon',  N'Kontak awal dengan HR, diminta mengirim proposal tertulis.', 2, 0),
(N'PT Jaya Manufaktur Otomasi',  N'staf2', -26, N'Surat',    N'Mengirim surat resmi permohonan kunjungan.', NULL, 1),
(N'PT Jaya Manufaktur Otomasi',  N'staf2',  -6, N'Tatap Muka', N'Bertemu HR Supervisor, kunjungan disetujui.', NULL, 1),
(N'PT Indra Perangkat Lunak',    N'staf',  -15, N'Email',    N'Perusahaan menolak karena jadwal penuh pada rentang tanggal usulan.', NULL, 1),
(N'PT Kirana Media Kreatif',     N'staf',  -10, N'WhatsApp', N'Kuota peserta yang ditawarkan terlalu kecil, pengajuan dibatalkan.', NULL, 1);

INSERT INTO dbo.Interaksi (PengajuanId, UserId, Tanggal, Media, Ringkasan, TanggalFollowUp, IsSelesai)
SELECT p.PengajuanId, u.UserId, DATEADD(DAY, i.HariOffset, SYSDATETIME()), i.Media, i.Ringkasan,
       CASE WHEN i.FollowUpOffset IS NULL THEN NULL
            ELSE DATEADD(DAY, i.FollowUpOffset, CAST(SYSDATETIME() AS DATE)) END,
       i.Selesai
FROM @i i
JOIN dbo.Perusahaan c ON c.Nama = i.Perusahaan
JOIN dbo.Pengajuan p ON p.PerusahaanId = c.PerusahaanId AND p.PeriodeId = @pAktif2
JOIN dbo.Users u ON u.Username = i.Pic;

/* ---------- Aktivitas terbaru ---------- */
INSERT INTO dbo.Aktivitas (UserId, JenisAksi, Entitas, EntitasId, Keterangan, Waktu)
SELECT u.UserId, a.Jenis, a.Entitas, NULL, a.Ket, DATEADD(HOUR, a.JamOffset, SYSDATETIME())
FROM (VALUES
    (N'staf2', N'Interaksi', N'Pengajuan', N'Menambah interaksi: PT Cakrawala Cloud Indonesia', -72),
    (N'staf',  N'Status',    N'Pengajuan', N'Mengubah status ke Dikonfirmasi: PT Arunika Data Teknologi', -96),
    (N'staf',  N'Status',    N'Pengajuan', N'Mengubah status ke Dikonfirmasi: PT Nusantara Digital Solusi', -120),
    (N'staf',  N'Interaksi', N'Pengajuan', N'Menambah interaksi: PT Eka Siber Aman', -48),
    (N'kadep', N'Tambah',    N'Pengajuan', N'Menambah pengajuan: PT Harapan Telekomunikasi Nusantara', -24)
) AS a (Username, Jenis, Entitas, Ket, JamOffset)
JOIN dbo.Users u ON u.Username = a.Username;
GO

PRINT N'Data seed dummy selesai dimasukkan.';
GO