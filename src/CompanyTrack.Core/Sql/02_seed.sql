/* =========================================================================
   CompanyTrack : data seed DUMMY untuk pengembangan, pengujian, dan demo
   Dijalankan otomatis sekali, saat file database pertama kali dibuat.
   Semua nama perusahaan dan akun adalah fiktif. Tanggal dihitung relatif
   terhadap hari ini supaya dashboard selalu punya data terlambat/hari ini.

   Akun demo (ubah sebelum dipakai sungguhan):
     kadep / kadep123   : KepalaDepartemen
     staf  / staf123    : Staf
     staf2 / staf123    : Staf
     bph   / bph123     : BPH (hanya baca)
   ========================================================================= */

/* ---------- Users (hash PBKDF2-SHA256, format PBKDF2$iterasi$salt$hash) ---------- */
INSERT INTO Users (Username, PasswordHash, Nama, Role, IsActive) VALUES
('kadep', 'PBKDF2$100000$06gjbvQSNdr6ZHHRy7YlUQ==$jidEX1l4vMfebORKh8fhfSbYxARgZ9InYD+v9p9CTjo=', 'Kepala Departemen External Affairs', 'KepalaDepartemen', 1),
('staf',  'PBKDF2$100000$NreTIN7fgRkpMr/OjfZYRw==$0olrMRnChV2PSSI0DpplSST66dB4GGYepfegxxW2qVk=', 'Staf External Affairs 1', 'Staf', 1),
('staf2', 'PBKDF2$100000$NreTIN7fgRkpMr/OjfZYRw==$0olrMRnChV2PSSI0DpplSST66dB4GGYepfegxxW2qVk=', 'Staf External Affairs 2', 'Staf', 1),
('bph',   'PBKDF2$100000$LomHhAbju+suxKn2CpB7FQ==$2q76rRZuoGaw7HXcs7QEmiiyx1wJkieGN/GwTF2qvio=', 'Perwakilan BPH', 'BPH', 1);

/* ---------- Master ---------- */
INSERT INTO AlasanPenolakan (Nama) VALUES
('Jadwal perusahaan penuh'),
('Kebijakan perusahaan tidak menerima kunjungan'),
('Kuota peserta tidak mencukupi'),
('Tidak ada respons'),
('Lainnya');

INSERT INTO Pengaturan (Kunci, Nilai) VALUES ('AmbangBalasanHari', '7');

INSERT INTO PeriodeKunjungan (Nama, TanggalKunjungan, TargetPerusahaan, IsAktif) VALUES
('Company Visit 2025', date('now', 'localtime', '-300 days'), 4, 0),
('Company Visit 2026', date('now', 'localtime', '55 days'),   5, 1);

INSERT INTO Perusahaan (Nama, BidangIndustri, Kota, Email, Telepon, NamaPIC, JabatanPIC) VALUES
('PT Nusantara Digital Solusi',   'Teknologi Informasi', 'Jakarta',    'hr@nusantara-digital.example', '021-5550101', 'Rina Wulandari',  'HR Business Partner'),
('PT Arunika Data Teknologi',     'Data dan AI',         'Surabaya',   'talent@arunika.example',       '031-5550102', 'Dimas Prakoso',   'Talent Acquisition'),
('PT Cakrawala Cloud Indonesia',  'Cloud Computing',     'Bandung',    'people@cakrawala.example',     '022-5550103', 'Maya Sari',       'People Operations'),
('PT Bahari Fintek Mandiri',      'Fintech',             'Jakarta',    'campus@bahari-fintek.example', '021-5550104', 'Anton Gunawan',   'Campus Relations'),
('PT Dwipa Game Studio',          'Game Development',    'Yogyakarta', 'studio@dwipa.example',         '0274-5550105', 'Putri Lestari',  'Studio Manager'),
('PT Eka Siber Aman',             'Keamanan Siber',      'Jakarta',    'hr@ekasiber.example',          '021-5550106', 'Fajar Nugroho',   'HR Manager'),
('PT Garuda Logistik Cerdas',     'E-Commerce dan Logistik', 'Surabaya', 'csr@garuda-logistik.example', '031-5550107', 'Hendra Wijaya',   'CSR Officer'),
('PT Harapan Telekomunikasi Nusantara', 'Telekomunikasi', 'Jakarta',   'edu@harapan-telko.example',    '021-5550108', 'Sinta Maharani',  'Education Program'),
('PT Indra Perangkat Lunak',      'Software House',      'Malang',     'hello@indra-pl.example',       '0341-5550109', 'Bayu Setiawan',   'Operations Lead'),
('PT Jaya Manufaktur Otomasi',    'Manufaktur dan IoT',  'Sidoarjo',   'hr@jaya-otomasi.example',      '031-5550110', 'Lina Marlina',    'HR Supervisor'),
('PT Kirana Media Kreatif',       'Media Digital',       'Surabaya',   'info@kirana-media.example',    '031-5550111', 'Rudi Hartono',    'Brand Manager'),
('PT Lintas Startup Raya',        'Startup',             'Denpasar',   'team@lintas-startup.example',  '0361-5550112', 'Nadia Pratiwi',   'Co-Founder');

/* ---------- Pengajuan periode 2026 (aktif) ---------- */
WITH s (Perusahaan, Pic, Status, UsulOffset, Kuota, KirimOffset, AktivitasOffset, Alasan) AS (VALUES
('PT Nusantara Digital Solusi',          'staf',  'Dikonfirmasi',     55, 30, -30,  -5, NULL),
('PT Arunika Data Teknologi',            'staf',  'Dikonfirmasi',     56, 25, -28,  -4, NULL),
('PT Jaya Manufaktur Otomasi',           'staf2', 'Dikonfirmasi',     57, 20, -26,  -6, NULL),
('PT Cakrawala Cloud Indonesia',         'staf2', 'NegosiasiJadwal',  58, 30, -20,  -3, NULL),
('PT Bahari Fintek Mandiri',             'staf',  'MenungguBalasan',  60, 20, -18, -12, NULL),
('PT Garuda Logistik Cerdas',            'staf2', 'MenungguBalasan',  62, 40, -14,  -9, NULL),
('PT Dwipa Game Studio',                 'staf2', 'SuratTerkirim',  NULL, NULL, -9,  -9, NULL),
('PT Eka Siber Aman',                    'staf',  'Dihubungi',      NULL, NULL, NULL, -2, NULL),
('PT Harapan Telekomunikasi Nusantara',  'kadep', 'Target',         NULL, NULL, NULL, -1, NULL),
('PT Indra Perangkat Lunak',             'staf',  'Ditolak',        NULL, NULL, -25, -15, 'Jadwal perusahaan penuh'),
('PT Kirana Media Kreatif',              'staf',  'Dibatalkan',     NULL, NULL, -22, -10, 'Kuota peserta tidak mencukupi')
)
INSERT INTO Pengajuan (PerusahaanId, PeriodeId, UserId, Status, TanggalUsulan, KuotaPeserta,
                       TanggalKirimSurat, TanggalAktivitasTerakhir, AlasanPenolakanId)
SELECT c.PerusahaanId, (SELECT PeriodeId FROM PeriodeKunjungan WHERE IsAktif = 1), u.UserId, s.Status,
       date('now', 'localtime', s.UsulOffset || ' days'),
       s.Kuota,
       date('now', 'localtime', s.KirimOffset || ' days'),
       datetime('now', 'localtime', s.AktivitasOffset || ' days'),
       a.AlasanId
FROM s
JOIN Perusahaan c ON c.Nama = s.Perusahaan
JOIN Users u ON u.Username = s.Pic
LEFT JOIN AlasanPenolakan a ON a.Nama = s.Alasan;

/* ---------- Pengajuan periode 2025 (riwayat) ---------- */
WITH h (Perusahaan, Pic, Status, UsulOffset, Kuota, AktivitasOffset, Alasan) AS (VALUES
('PT Nusantara Digital Solusi', 'staf',  'Terlaksana', -300, 25, -310, NULL),
('PT Lintas Startup Raya',      'staf2', 'Terlaksana', -300, 20, -312, NULL),
('PT Indra Perangkat Lunak',    'staf',  'Ditolak',    NULL, NULL, -340, 'Kebijakan perusahaan tidak menerima kunjungan'),
('PT Eka Siber Aman',           'staf2', 'Ditolak',    NULL, NULL, -335, 'Tidak ada respons')
)
INSERT INTO Pengajuan (PerusahaanId, PeriodeId, UserId, Status, TanggalUsulan, KuotaPeserta,
                       TanggalKirimSurat, TanggalAktivitasTerakhir, AlasanPenolakanId)
SELECT c.PerusahaanId, (SELECT PeriodeId FROM PeriodeKunjungan WHERE IsAktif = 0), u.UserId, h.Status,
       date('now', 'localtime', h.UsulOffset || ' days'),
       h.Kuota, NULL,
       datetime('now', 'localtime', h.AktivitasOffset || ' days'),
       a.AlasanId
FROM h
JOIN Perusahaan c ON c.Nama = h.Perusahaan
JOIN Users u ON u.Username = h.Pic
LEFT JOIN AlasanPenolakan a ON a.Nama = h.Alasan;

/* Riwayat status: satu baris Target lalu satu baris menuju status terkini */
INSERT INTO RiwayatStatus (PengajuanId, StatusLama, StatusBaru, Waktu, UserId)
SELECT p.PengajuanId, NULL, 'Target', datetime(p.TanggalAktivitasTerakhir, '-2 days'), p.UserId
FROM Pengajuan p;

INSERT INTO RiwayatStatus (PengajuanId, StatusLama, StatusBaru, Waktu, UserId)
SELECT p.PengajuanId, 'Target', p.Status, p.TanggalAktivitasTerakhir, p.UserId
FROM Pengajuan p
WHERE p.Status <> 'Target';

/* ---------- Interaksi dan follow-up ---------- */
WITH i (Perusahaan, Pic, HariOffset, Media, Ringkasan, FollowUpOffset, Selesai) AS (VALUES
('PT Nusantara Digital Solusi', 'staf',  -30, 'Surat',    'Mengirim surat resmi permohonan company visit.', NULL, 1),
('PT Nusantara Digital Solusi', 'staf',   -5, 'Email',    'Perusahaan mengonfirmasi tanggal kunjungan.', NULL, 1),
('PT Arunika Data Teknologi',   'staf',  -28, 'Email',    'Mengirim proposal kunjungan ke tim talent acquisition.', NULL, 1),
('PT Arunika Data Teknologi',   'staf',   -4, 'Telepon',  'Konfirmasi tanggal dan kuota peserta.', NULL, 1),
('PT Cakrawala Cloud Indonesia','staf2', -20, 'Email',    'Mengirim surat resmi permohonan kunjungan.', -13, 1),
('PT Cakrawala Cloud Indonesia','staf2',  -3, 'WhatsApp', 'Perusahaan menawarkan dua opsi tanggal, menunggu keputusan internal HMTC.', 0, 0),
('PT Bahari Fintek Mandiri',    'staf',  -18, 'Surat',    'Surat permohonan kunjungan diterima resepsionis.', -12, 1),
('PT Bahari Fintek Mandiri',    'staf',  -12, 'Email',    'Follow-up pertama, belum ada balasan.', -5, 0),
('PT Garuda Logistik Cerdas',   'staf2', -14, 'Email',    'Mengirim proposal kunjungan ke CSR Officer.', -7, 1),
('PT Garuda Logistik Cerdas',   'staf2',  -9, 'Telepon',  'Telepon tidak diangkat, akan dicoba lagi.', 3, 0),
('PT Dwipa Game Studio',        'staf2',  -9, 'Email',    'Surat terkirim ke studio manager.', -2, 0),
('PT Eka Siber Aman',           'staf',   -2, 'Telepon',  'Kontak awal dengan HR, diminta mengirim proposal tertulis.', 2, 0),
('PT Jaya Manufaktur Otomasi',  'staf2', -26, 'Surat',    'Mengirim surat resmi permohonan kunjungan.', NULL, 1),
('PT Jaya Manufaktur Otomasi',  'staf2',  -6, 'Tatap Muka', 'Bertemu HR Supervisor, kunjungan disetujui.', NULL, 1),
('PT Indra Perangkat Lunak',    'staf',  -15, 'Email',    'Perusahaan menolak karena jadwal penuh pada rentang tanggal usulan.', NULL, 1),
('PT Kirana Media Kreatif',     'staf',  -10, 'WhatsApp', 'Kuota peserta yang ditawarkan terlalu kecil, pengajuan dibatalkan.', NULL, 1)
)
INSERT INTO Interaksi (PengajuanId, UserId, Tanggal, Media, Ringkasan, TanggalFollowUp, IsSelesai)
SELECT p.PengajuanId, u.UserId, datetime('now', 'localtime', i.HariOffset || ' days'), i.Media, i.Ringkasan,
       date('now', 'localtime', i.FollowUpOffset || ' days'),
       i.Selesai
FROM i
JOIN Perusahaan c ON c.Nama = i.Perusahaan
JOIN Pengajuan p ON p.PerusahaanId = c.PerusahaanId
                AND p.PeriodeId = (SELECT PeriodeId FROM PeriodeKunjungan WHERE IsAktif = 1)
JOIN Users u ON u.Username = i.Pic;

/* ---------- Aktivitas terbaru ---------- */
WITH a (Username, Jenis, Entitas, Ket, JamOffset) AS (VALUES
('staf2', 'Interaksi', 'Pengajuan', 'Menambah interaksi: PT Cakrawala Cloud Indonesia', -72),
    ('staf',  'Status',    'Pengajuan', 'Mengubah status ke Dikonfirmasi: PT Arunika Data Teknologi', -96),
    ('staf',  'Status',    'Pengajuan', 'Mengubah status ke Dikonfirmasi: PT Nusantara Digital Solusi', -120),
    ('staf',  'Interaksi', 'Pengajuan', 'Menambah interaksi: PT Eka Siber Aman', -48),
    ('kadep', 'Tambah',    'Pengajuan', 'Menambah pengajuan: PT Harapan Telekomunikasi Nusantara', -24)
)
INSERT INTO Aktivitas (UserId, JenisAksi, Entitas, EntitasId, Keterangan, Waktu)
SELECT u.UserId, a.Jenis, a.Entitas, NULL, a.Ket, datetime('now', 'localtime', a.JamOffset || ' hours')
FROM a
JOIN Users u ON u.Username = a.Username;
