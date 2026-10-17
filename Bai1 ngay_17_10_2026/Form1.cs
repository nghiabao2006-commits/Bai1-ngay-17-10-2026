using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Bai1_ngay_17_10_2026
{
    public partial class MainForm : Form
    {
        private readonly List<Appointment> _all = new List<Appointment>();
        private BindingList<Appointment> _view = new BindingList<Appointment>();
        private Appointment _selected;           // dòng đang chỉnh sửa (may be null)
        private static readonly string[] Statuses = { "Chờ khám", "Đang khám", "Hoàn tất", "Đã hủy" };

        public MainForm()
        {
            InitializeComponent();
            InitControls();
            InitGrid();
            SeedData();
            WireEvents();
            ApplyFilter();
        }

        // ---------- Khởi tạo ----------
        private void InitControls()
        {
            cboDoctor.Items.AddRange(new object[] { "BS. Nguyễn Văn An", "BS. Trần Thị Bình", "BS. Lê Minh Châu" });
            cboDoctor.SelectedIndex = 0;

            cboStatus.Items.AddRange(Statuses);
            cboStatus.SelectedIndex = 0;

            cboFilterStatus.Items.Add("Tất cả");
            cboFilterStatus.Items.AddRange(Statuses);
            cboFilterStatus.SelectedIndex = 0;

            dtpAppointment.Format = DateTimePickerFormat.Custom;
            dtpAppointment.CustomFormat = "dd/MM/yyyy HH:mm";
            dtpAppointment.Value = DateTime.Now.AddHours(1);

            dtpFilterDate.Format = DateTimePickerFormat.Short;
            dtpFilterDate.Enabled = false;
        }

        private void InitGrid()
        {
            dgv.AutoGenerateColumns = false;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.MultiSelect = false;
            dgv.ReadOnly = true;
            dgv.AllowUserToAddRows = false;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            dgv.Columns.Add(Col("Id", "Mã lịch hẹn"));
            dgv.Columns.Add(Col("PatientName", "Bệnh nhân"));
            dgv.Columns.Add(Col("Phone", "SĐT"));
            var colTime = Col("AppointmentTime", "Ngày giờ hẹn");
            colTime.DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
            dgv.Columns.Add(colTime);
            dgv.Columns.Add(Col("Doctor", "Bác sĩ"));
            dgv.Columns.Add(Col("Reason", "Lý do khám"));
            dgv.Columns.Add(Col("Status", "Trạng thái"));

            bindingSource1.DataSource = _view;
            dgv.DataSource = bindingSource1;
        }

        private static System.Windows.Forms.DataGridViewTextBoxColumn Col(string prop, string header)
        {
            return new System.Windows.Forms.DataGridViewTextBoxColumn { DataPropertyName = prop, HeaderText = header, Name = "col" + prop };
        }

        private void SeedData()
        {
            _all.Add(new Appointment { Id = "LH001", PatientName = "Nguyễn Văn A", Phone = "0901234567",
                AppointmentTime = DateTime.Today.AddHours(DateTime.Now.Hour + 2), Doctor = "BS. Nguyễn Văn An",
                Reason = "Khám tổng quát", Status = "Chờ khám" });
            _all.Add(new Appointment { Id = "LH002", PatientName = "Trần Thị B", Phone = "0912345678",
                AppointmentTime = DateTime.Today.AddDays(1).AddHours(9), Doctor = "BS. Trần Thị Bình",
                Reason = "Đau đầu", Status = "Chờ khám" });
        }

        private void WireEvents()
        {
            btnAdd.Click += BtnAdd_Click;
            btnUpdate.Click += BtnUpdate_Click;
            btnDelete.Click += BtnDelete_Click;
            btnClear.Click += (s, e) => ClearForm();
            btnResetFilter.Click += BtnResetFilter_Click;

            txtSearch.TextChanged += (s, e) => ApplyFilter();      // tìm kiếm tức thời
            cboFilterStatus.SelectedIndexChanged += (s, e) => ApplyFilter();
            dtpFilterDate.ValueChanged += (s, e) => ApplyFilter();
            chkFilterDate.CheckedChanged += (s, e) =>
            {
                dtpFilterDate.Enabled = chkFilterDate.Checked;
                ApplyFilter();
            };

            dgv.SelectionChanged += Dgv_SelectionChanged;
            mnuExport.Click += MnuExport_Click;
            mnuExit.Click += (s, e) => Close();
        }

        // ---------- Lọc + tìm kiếm ----------
        private void ApplyFilter()
        {
            string kw = txtSearch.Text.Trim();
            string status = cboFilterStatus.SelectedItem?.ToString() ?? "Tất cả";

            var result = _all.Where(a =>
                (kw.Length == 0 ||
                 (a.PatientName != null && a.PatientName.IndexOf(kw, StringComparison.CurrentCultureIgnoreCase) >= 0) ||
                 (a.Phone != null && a.Phone.IndexOf(kw, StringComparison.CurrentCultureIgnoreCase) >= 0)) &&
                (!chkFilterDate.Checked || a.AppointmentTime.Date == dtpFilterDate.Value.Date) &&
                (status == "Tất cả" || a.Status == status))
                .OrderBy(a => a.AppointmentTime);

            _view = new BindingList<Appointment>(result.ToList());
            bindingSource1.DataSource = _view;
            dgv.ClearSelection();

            lblCount.Text = $"Số lịch hẹn: {_view.Count} (tổng: {_all.Count})";
        }

        private void BtnResetFilter_Click(object sender, EventArgs e)
        {
            txtSearch.Clear();
            chkFilterDate.Checked = false;
            cboFilterStatus.SelectedIndex = 0;
            ApplyFilter();
        }

        // ---------- Chọn dòng -> nạp lên form ----------
        private void Dgv_SelectionChanged(object sender, EventArgs e)
        {
            if (dgv.SelectedRows.Count == 0) return;
            var item = dgv.SelectedRows[0].DataBoundItem as Appointment;
            if (item == null) return;

            _selected = item;
            errorProvider1.Clear();
            txtId.Text = item.Id;
            txtId.ReadOnly = true;                 // không cho đổi mã khi sửa
            txtName.Text = item.PatientName;
            txtPhone.Text = item.Phone;
            dtpAppointment.Value = item.AppointmentTime;
            cboDoctor.SelectedItem = item.Doctor;
            txtReason.Text = item.Reason;
            cboStatus.SelectedItem = item.Status;
        }

        private void ClearForm()
        {
            _selected = null;
            errorProvider1.Clear();
            txtId.Clear(); txtId.ReadOnly = false;
            txtName.Clear(); txtPhone.Clear(); txtReason.Clear();
            dtpAppointment.Value = DateTime.Now.AddHours(1);
            cboDoctor.SelectedIndex = 0;
            cboStatus.SelectedIndex = 0;
            dgv.ClearSelection();
            txtId.Focus();
        }

        // ---------- Thêm / Sửa / Xóa ----------
        private void BtnAdd_Click(object sender, EventArgs e)
        {
            if (!ValidateInput(isNew: true)) return;

            _all.Add(ReadForm(new Appointment()));
            ApplyFilter();
            ClearForm();
        }

        private void BtnUpdate_Click(object sender, EventArgs e)
        {
            if (_selected == null)
            {
                MessageBox.Show("Hãy chọn một lịch hẹn trong danh sách.", "Thông báo");
                return;
            }
            if (!ValidateInput(isNew: false)) return;

            if (MessageBox.Show("Bạn có chắc muốn cập nhật lịch hẹn này?", "Xác nhận",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

            ReadForm(_selected);
            ApplyFilter();
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (_selected == null)
            {
                MessageBox.Show("Hãy chọn một lịch hẹn để xóa.", "Thông báo");
                return;
            }
            if (MessageBox.Show($"Xóa lịch hẹn {_selected.Id}?", "Xác nhận",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;

            _all.Remove(_selected);
            ApplyFilter();
            ClearForm();
        }

        private Appointment ReadForm(Appointment a)
        {
            if (_selected == null) a.Id = txtId.Text.Trim();   // mã chỉ gán khi thêm mới
            a.PatientName = txtName.Text.Trim();
            a.Phone = txtPhone.Text.Trim();
            a.AppointmentTime = dtpAppointment.Value;
            a.Doctor = cboDoctor.SelectedItem?.ToString() ?? "";
            a.Reason = txtReason.Text.Trim();
            a.Status = cboStatus.SelectedItem?.ToString() ?? Statuses[0];
            return a;
        }

        // ---------- Kiểm tra dữ liệu ----------
        private bool ValidateInput(bool isNew)
        {
            errorProvider1.Clear();
            bool ok = true;

            string id = txtId.Text.Trim();
            if (id.Length == 0)
                ok = SetError(txtId, "Mã lịch hẹn không được để trống.");
            else if (isNew && _all.Any(a => a.Id.Equals(id, StringComparison.OrdinalIgnoreCase)))
                ok = SetError(txtId, "Mã lịch hẹn đã tồn tại.");

            if (txtName.Text.Trim().Length == 0)
                ok = SetError(txtName, "Vui lòng nhập họ tên bệnh nhân.");

            if (!Regex.IsMatch(txtPhone.Text.Trim(), @"^(0|\+84)\d{9}$"))
                ok = SetError(txtPhone, "SĐT không hợp lệ (VD: 0901234567).");

            if (txtReason.Text.Trim().Length == 0)
                ok = SetError(txtReason, "Vui lòng nhập lý do khám.");

            bool timeChanged = isNew || _selected == null || dtpAppointment.Value != _selected.AppointmentTime;
            if (timeChanged && dtpAppointment.Value < DateTime.Now)
                ok = SetError(dtpAppointment, "Ngày giờ hẹn không được ở quá khứ.");

            return ok;
        }

        private bool SetError(Control c, string msg)
        {
            errorProvider1.SetError(c, msg);
            return false;
        }

        // ---------- Xuất CSV ----------
        private void MnuExport_Click(object sender, EventArgs e)
        {
            using (var dlg = new SaveFileDialog())
            {
                dlg.Filter = "CSV (*.csv)|*.csv";
                dlg.FileName = string.Format("lichhen_{0:yyyyMMdd_HHmm}.csv", DateTime.Now);
                if (dlg.ShowDialog() != DialogResult.OK) return;

                var sb = new StringBuilder();
                sb.AppendLine("Mã,Bệnh nhân,SĐT,Ngày giờ,Bác sĩ,Lý do,Trạng thái");
                foreach (var a in _view)
                {
                    sb.AppendLine(string.Join(",", new[]
                    {
                        a.Id, a.PatientName, "'" + a.Phone,
                        a.AppointmentTime.ToString("dd/MM/yyyy HH:mm"),
                        a.Doctor, a.Reason, a.Status
                    }.Select(Csv)));
                }
                System.IO.File.WriteAllText(dlg.FileName, sb.ToString(), new UTF8Encoding(true));
                MessageBox.Show(string.Format("Đã xuất {0} lịch hẹn.", _view.Count), "Thành công");
            }
        }

        private static string Csv(string s) => "\"" + s.Replace("\"", "\"\"") + "\"";

        private void tlpInput_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
