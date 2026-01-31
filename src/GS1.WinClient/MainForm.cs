using GS1.Core.DTOs;
using GS1.Core.Entities;
using GS1.WinClient.Services;

namespace GS1.WinClient;

public partial class MainForm : Form
{
    private readonly ApiClient _apiClient;
    
    // Tab sayfaları için kontroller
    private TabControl tabControl = null!;
    private TabPage tabCustomers = null!;
    private TabPage tabProducts = null!;
    private TabPage tabWorkOrders = null!;
    private TabPage tabAggregation = null!;

    // Customer Tab kontrolleri
    private DataGridView dgvCustomers = null!;
    private TextBox txtCompanyName = null!;
    private TextBox txtGLN = null!;
    private TextBox txtCustomerDescription = null!;
    private Button btnAddCustomer = null!;
    private Button btnRefreshCustomers = null!;

    // Product Tab kontrolleri
    private DataGridView dgvProducts = null!;
    private ComboBox cmbCustomerForProduct = null!;
    private TextBox txtGTIN = null!;
    private TextBox txtProductName = null!;
    private Button btnAddProduct = null!;
    private Button btnRefreshProducts = null!;

    // WorkOrder Tab kontrolleri
    private DataGridView dgvWorkOrders = null!;
    private ComboBox cmbProductForWorkOrder = null!;
    private TextBox txtWorkOrderNumber = null!;
    private NumericUpDown numQuantity = null!;
    private TextBox txtBatchNumber = null!;
    private DateTimePicker dtpExpirationDate = null!;
    private NumericUpDown numItemsPerBox = null!;
    private NumericUpDown numBoxesPerPallet = null!;
    private Button btnCreateWorkOrder = null!;
    private Button btnGenerateSerials = null!;
    private Button btnViewDetails = null!;
    private Button btnRefreshWorkOrders = null!;
    private NumericUpDown numSerialCount = null!;
    private DataGridView dgvSerialNumbers = null!;
    private RichTextBox rtbWorkOrderDetails = null!;

    // Aggregation Tab kontrolleri
    private ComboBox cmbWorkOrderForAggregation = null!;
    private DataGridView dgvBoxes = null!;
    private DataGridView dgvPallets = null!;
    private DataGridView dgvUnassignedItems = null!;
    private Button btnCreateBox = null!;
    private Button btnCreatePallet = null!;
    private Button btnAddToBox = null!;
    private Button btnAddToPallet = null!;
    private TreeView tvAggregationHierarchy = null!;

    public MainForm(ApiClient apiClient)
    {
        _apiClient = apiClient;
        InitializeComponent();
        ApplyTheme();
    }

    // Kurumsal Tema Renkleri
    private static readonly Color PrimaryColor = Color.FromArgb(33, 37, 41);        // Koyu Gri (Başlıklar)
    private static readonly Color SecondaryColor = Color.FromArgb(52, 58, 64);      // Orta Koyu Gri
    private static readonly Color AccentColor = Color.FromArgb(13, 110, 253);       // Kurumsal Mavi
    private static readonly Color SuccessColor = Color.FromArgb(25, 135, 84);       // Kurumsal Yeşil
    private static readonly Color WarningColor = Color.FromArgb(255, 193, 7);       // Uyarı Sarı
    private static readonly Color BackgroundColor = Color.FromArgb(248, 249, 250);  // Açık Gri Arka Plan
    private static readonly Color CardColor = Color.White;                           // Kart Arka Plan
    private static readonly Color HeaderColor = Color.FromArgb(233, 236, 239);      // Tablo Başlık
    private static readonly Color AlternateRowColor = Color.FromArgb(248, 249, 250);// Alternatif Satır
    private static readonly Color BorderColor = Color.FromArgb(206, 212, 218);      // Kenar Rengi
    private static readonly Color TextColor = Color.FromArgb(33, 37, 41);           // Ana Metin
    private static readonly Color TextMutedColor = Color.FromArgb(108, 117, 125);   // Soluk Metin

    private void ApplyTheme()
    {
        this.BackColor = BackgroundColor;
        this.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
        this.ForeColor = TextColor;
        
        // Tüm kontrollere stil uygula
        ApplyStyleToControls(this.Controls);
    }

    private void ApplyStyleToControls(Control.ControlCollection controls)
    {
        foreach (Control control in controls)
        {
            switch (control)
            {
                case Button btn:
                    StyleButton(btn);
                    break;
                case DataGridView dgv:
                    StyleDataGridView(dgv);
                    break;
                case TabControl tab:
                    tab.Font = new Font("Segoe UI", 9.5F, FontStyle.Regular);
                    tab.Padding = new Point(12, 6);
                    break;
                case TextBox txt:
                    txt.BorderStyle = BorderStyle.FixedSingle;
                    txt.Font = new Font("Segoe UI", 9.5F);
                    break;
                case ComboBox cmb:
                    cmb.FlatStyle = FlatStyle.Flat;
                    cmb.Font = new Font("Segoe UI", 9.5F);
                    break;
                case NumericUpDown num:
                    num.BorderStyle = BorderStyle.FixedSingle;
                    num.Font = new Font("Segoe UI", 9.5F);
                    break;
                case TreeView tv:
                    tv.BorderStyle = BorderStyle.FixedSingle;
                    tv.Font = new Font("Segoe UI", 9.5F);
                    tv.BackColor = CardColor;
                    tv.LineColor = BorderColor;
                    break;
                case RichTextBox rtb:
                    rtb.BorderStyle = BorderStyle.FixedSingle;
                    rtb.Font = new Font("Cascadia Code", 9F);
                    rtb.BackColor = Color.FromArgb(40, 44, 52);  // Dark theme for code
                    rtb.ForeColor = Color.FromArgb(171, 178, 191);
                    break;
                case Label lbl:
                    lbl.ForeColor = TextColor;
                    break;
                case GroupBox grp:
                    grp.ForeColor = TextColor;
                    grp.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                    break;
                case Panel pnl:
                    if (pnl.BorderStyle == BorderStyle.FixedSingle)
                        pnl.BackColor = CardColor;
                    break;
            }

            // Alt kontrollere de uygula
            if (control.HasChildren)
                ApplyStyleToControls(control.Controls);
        }
    }

    private void StyleButton(Button btn)
    {
        btn.FlatStyle = FlatStyle.Flat;
        btn.FlatAppearance.BorderSize = 1;
        btn.Cursor = Cursors.Hand;
        btn.Font = new Font("Segoe UI", 9F, FontStyle.Regular);

        // Buton türüne göre renk
        if (btn.Text.Contains("Ekle") || btn.Text.Contains("Oluştur") || btn.Text.Contains("Üret"))
        {
            btn.BackColor = AccentColor;
            btn.ForeColor = Color.White;
            btn.FlatAppearance.BorderColor = AccentColor;
        }
        else if (btn.Text.Contains("Yenile"))
        {
            btn.BackColor = CardColor;
            btn.ForeColor = SecondaryColor;
            btn.FlatAppearance.BorderColor = BorderColor;
        }
        else if (btn.Text.Contains("Göster") || btn.Text.Contains("Detay"))
        {
            btn.BackColor = SecondaryColor;
            btn.ForeColor = Color.White;
            btn.FlatAppearance.BorderColor = SecondaryColor;
        }
        else
        {
            btn.BackColor = SuccessColor;
            btn.ForeColor = Color.White;
            btn.FlatAppearance.BorderColor = SuccessColor;
        }
    }

    private void StyleDataGridView(DataGridView dgv)
    {
        dgv.BorderStyle = BorderStyle.FixedSingle;
        dgv.BackgroundColor = CardColor;
        dgv.GridColor = BorderColor;
        dgv.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        dgv.RowHeadersVisible = false;
        
        // Header stili
        dgv.EnableHeadersVisualStyles = false;
        dgv.ColumnHeadersDefaultCellStyle.BackColor = HeaderColor;
        dgv.ColumnHeadersDefaultCellStyle.ForeColor = TextColor;
        dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        dgv.ColumnHeadersDefaultCellStyle.Padding = new Padding(8, 4, 8, 4);
        dgv.ColumnHeadersHeight = 38;
        dgv.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
        
        // Row stili
        dgv.DefaultCellStyle.BackColor = CardColor;
        dgv.DefaultCellStyle.ForeColor = TextColor;
        dgv.DefaultCellStyle.SelectionBackColor = AccentColor;
        dgv.DefaultCellStyle.SelectionForeColor = Color.White;
        dgv.DefaultCellStyle.Padding = new Padding(8, 4, 8, 4);
        dgv.DefaultCellStyle.Font = new Font("Segoe UI", 9F);
        dgv.RowTemplate.Height = 32;
        dgv.AlternatingRowsDefaultCellStyle.BackColor = AlternateRowColor;
    }

    private void InitializeComponent()
    {
        this.Text = "GS1 L3 Serilizasyon ve Agregasyon Sistemi";
        this.Size = new Size(1400, 900);
        this.StartPosition = FormStartPosition.CenterScreen;

        // Ana TabControl
        tabControl = new TabControl
        {
            Dock = DockStyle.Fill
        };

        // Tab sayfaları
        CreateCustomersTab();
        CreateProductsTab();
        CreateWorkOrdersTab();
        CreateAggregationTab();

        tabControl.TabPages.Add(tabCustomers);
        tabControl.TabPages.Add(tabProducts);
        tabControl.TabPages.Add(tabWorkOrders);
        tabControl.TabPages.Add(tabAggregation);

        this.Controls.Add(tabControl);

        // Form yüklendiğinde verileri getir
        this.Load += MainForm_Load;
    }

    private async void MainForm_Load(object? sender, EventArgs e)
    {
        await LoadCustomersAsync();
        await LoadCustomerComboAsync();
        await LoadProductsAsync();
        await LoadProductComboAsync();
        await LoadWorkOrdersAsync();
        await LoadWorkOrderComboAsync();
    }

    #region Customers Tab

    private void CreateCustomersTab()
    {
        tabCustomers = new TabPage("Müşteriler");

        var splitContainer = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Horizontal,
            SplitterDistance = 400
        };

        // Üst panel - Grid
        dgvCustomers = new DataGridView
        {
            Dock = DockStyle.Fill,
            AutoGenerateColumns = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            AllowUserToAddRows = false,
            ReadOnly = true
        };

        dgvCustomers.Columns.AddRange(
            new DataGridViewTextBoxColumn { Name = "Id", HeaderText = "ID", DataPropertyName = "Id", Width = 50 },
            new DataGridViewTextBoxColumn { Name = "CompanyName", HeaderText = "Firma Adı", DataPropertyName = "CompanyName", Width = 200 },
            new DataGridViewTextBoxColumn { Name = "GLN", HeaderText = "GLN", DataPropertyName = "GLN", Width = 150 },
            new DataGridViewTextBoxColumn { Name = "Description", HeaderText = "Açıklama", DataPropertyName = "Description", Width = 200 },
            new DataGridViewTextBoxColumn { Name = "CreatedAt", HeaderText = "Oluşturma Tarihi", DataPropertyName = "CreatedAt", Width = 150 }
        );

        splitContainer.Panel1.Controls.Add(dgvCustomers);

        // Alt panel - Form
        var formPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };

        var lblCompanyName = new Label { Text = "Firma Adı:", Location = new Point(10, 10), Size = new Size(120, 20), AutoSize = false };
        txtCompanyName = new TextBox { Location = new Point(140, 10), Size = new Size(250, 25) };

        var lblGLN = new Label { Text = "GLN (13 hane):", Location = new Point(10, 45), Size = new Size(120, 20), AutoSize = false };
        txtGLN = new TextBox { Location = new Point(140, 45), Size = new Size(150, 25), MaxLength = 13 };
        var tipGLN = new ToolTip();
        tipGLN.SetToolTip(txtGLN, "13 haneli Global Location Number (GLN)\nGS1 tarafından atanmış numara\nÖrnek: 1234567890128");

        var lblDescription = new Label { Text = "Açıklama:", Location = new Point(10, 80), Size = new Size(120, 20), AutoSize = false };
        txtCustomerDescription = new TextBox { Location = new Point(140, 80), Size = new Size(250, 25) };

        btnAddCustomer = new Button { Text = "Müşteri Ekle", Location = new Point(120, 120), Size = new Size(120, 30) };
        btnAddCustomer.Click += BtnAddCustomer_Click;

        btnRefreshCustomers = new Button { Text = "Yenile", Location = new Point(250, 120), Size = new Size(80, 30) };
        btnRefreshCustomers.Click += async (s, e) => await LoadCustomersAsync();

        formPanel.Controls.AddRange(new Control[] { lblCompanyName, txtCompanyName, lblGLN, txtGLN, lblDescription, txtCustomerDescription, btnAddCustomer, btnRefreshCustomers });

        splitContainer.Panel2.Controls.Add(formPanel);
        tabCustomers.Controls.Add(splitContainer);
    }

    private async void BtnAddCustomer_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtCompanyName.Text) || string.IsNullOrWhiteSpace(txtGLN.Text))
        {
            MessageBox.Show("Firma adı ve GLN alanları zorunludur.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var dto = new CustomerCreateDto(txtCompanyName.Text, txtGLN.Text, txtCustomerDescription.Text);
        var result = await _apiClient.CreateCustomerAsync(dto);

        if (result?.Success == true)
        {
            MessageBox.Show("Müşteri başarıyla eklendi.", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
            txtCompanyName.Clear();
            txtGLN.Clear();
            txtCustomerDescription.Clear();
            await LoadCustomersAsync();
            await LoadCustomerComboAsync();
        }
        else
        {
            MessageBox.Show(result?.Message ?? "Hata oluştu", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task LoadCustomersAsync()
    {
        var result = await _apiClient.GetCustomersAsync();
        if (result?.Success == true && result.Data != null)
        {
            dgvCustomers.DataSource = result.Data.ToList();
        }
    }

    #endregion

    #region Products Tab

    private void CreateProductsTab()
    {
        tabProducts = new TabPage("Ürünler");

        var splitContainer = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Horizontal,
            SplitterDistance = 400
        };

        // Üst panel - Grid
        dgvProducts = new DataGridView
        {
            Dock = DockStyle.Fill,
            AutoGenerateColumns = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            AllowUserToAddRows = false,
            ReadOnly = true
        };

        dgvProducts.Columns.AddRange(
            new DataGridViewTextBoxColumn { Name = "Id", HeaderText = "ID", DataPropertyName = "Id", Width = 50 },
            new DataGridViewTextBoxColumn { Name = "GTIN", HeaderText = "GTIN", DataPropertyName = "GTIN", Width = 150 },
            new DataGridViewTextBoxColumn { Name = "ProductName", HeaderText = "Ürün Adı", DataPropertyName = "ProductName", Width = 200 },
            new DataGridViewTextBoxColumn { Name = "CustomerName", HeaderText = "Müşteri", DataPropertyName = "CustomerName", Width = 150 },
            new DataGridViewTextBoxColumn { Name = "CreatedAt", HeaderText = "Oluşturma Tarihi", DataPropertyName = "CreatedAt", Width = 150 }
        );

        splitContainer.Panel1.Controls.Add(dgvProducts);

        // Alt panel - Form
        var formPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };

        var lblCustomer = new Label { Text = "Müşteri:", Location = new Point(10, 10), Size = new Size(120, 20), AutoSize = false };
        cmbCustomerForProduct = new ComboBox { Location = new Point(140, 10), Size = new Size(250, 25), DropDownStyle = ComboBoxStyle.DropDownList };

        var lblGTIN = new Label { Text = "GTIN (13 hane):", Location = new Point(10, 45), Size = new Size(120, 20), AutoSize = false };
        txtGTIN = new TextBox { Location = new Point(140, 45), Size = new Size(150, 25), MaxLength = 13 };
        var tipGTIN = new ToolTip();
        tipGTIN.SetToolTip(txtGTIN, "13 haneli Global Trade Item Number (GTIN-13/EAN-13)\nGS1 tarafından atanmış ürün numarası\nÖrnek: 5901234123457");

        var lblProductName = new Label { Text = "Ürün Adı:", Location = new Point(10, 80), Size = new Size(120, 20), AutoSize = false };
        txtProductName = new TextBox { Location = new Point(140, 80), Size = new Size(250, 25) };

        btnAddProduct = new Button { Text = "Ürün Ekle", Location = new Point(120, 120), Size = new Size(120, 30) };
        btnAddProduct.Click += BtnAddProduct_Click;

        btnRefreshProducts = new Button { Text = "Yenile", Location = new Point(250, 120), Size = new Size(80, 30) };
        btnRefreshProducts.Click += async (s, e) => await LoadProductsAsync();

        formPanel.Controls.AddRange(new Control[] { lblCustomer, cmbCustomerForProduct, lblGTIN, txtGTIN, lblProductName, txtProductName, btnAddProduct, btnRefreshProducts });

        splitContainer.Panel2.Controls.Add(formPanel);
        tabProducts.Controls.Add(splitContainer);
    }

    private async void BtnAddProduct_Click(object? sender, EventArgs e)
    {
        if (cmbCustomerForProduct.SelectedItem == null || string.IsNullOrWhiteSpace(txtGTIN.Text) || string.IsNullOrWhiteSpace(txtProductName.Text))
        {
            MessageBox.Show("Tüm alanlar zorunludur.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var customer = (CustomerDto)cmbCustomerForProduct.SelectedItem;
        var dto = new ProductCreateDto(txtGTIN.Text, txtProductName.Text, null, customer.Id);
        var result = await _apiClient.CreateProductAsync(dto);

        if (result?.Success == true)
        {
            MessageBox.Show("Ürün başarıyla eklendi.", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
            txtGTIN.Clear();
            txtProductName.Clear();
            await LoadProductsAsync();
            await LoadProductComboAsync();
        }
        else
        {
            MessageBox.Show(result?.Message ?? "Hata oluştu", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task LoadProductsAsync()
    {
        var result = await _apiClient.GetProductsAsync();
        if (result?.Success == true && result.Data != null)
        {
            dgvProducts.DataSource = result.Data.ToList();
        }
        await LoadCustomerComboAsync();
    }

    private async Task LoadCustomerComboAsync()
    {
        var result = await _apiClient.GetCustomersAsync();
        if (result?.Success == true && result.Data != null)
        {
            cmbCustomerForProduct.DataSource = result.Data.ToList();
            cmbCustomerForProduct.DisplayMember = "CompanyName";
            cmbCustomerForProduct.ValueMember = "Id";
        }
    }

    #endregion

    #region WorkOrders Tab

    private void CreateWorkOrdersTab()
    {
        tabWorkOrders = new TabPage("İş Emirleri");

        var mainSplit = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Vertical,
            SplitterDistance = 700
        };

        // Sol panel - İş emirleri ve form
        var leftSplit = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Horizontal,
            SplitterDistance = 300
        };

        // İş emirleri grid
        dgvWorkOrders = new DataGridView
        {
            Dock = DockStyle.Fill,
            AutoGenerateColumns = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            AllowUserToAddRows = false,
            ReadOnly = true
        };

        dgvWorkOrders.Columns.AddRange(
            new DataGridViewTextBoxColumn { Name = "Id", HeaderText = "ID", DataPropertyName = "Id", Width = 50 },
            new DataGridViewTextBoxColumn { Name = "WorkOrderNumber", HeaderText = "İş Emri No", DataPropertyName = "WorkOrderNumber", Width = 120 },
            new DataGridViewTextBoxColumn { Name = "ProductName", HeaderText = "Ürün", DataPropertyName = "ProductName", Width = 150 },
            new DataGridViewTextBoxColumn { Name = "ProductionQuantity", HeaderText = "Adet", DataPropertyName = "ProductionQuantity", Width = 70 },
            new DataGridViewTextBoxColumn { Name = "BatchNumber", HeaderText = "Batch", DataPropertyName = "BatchNumber", Width = 100 },
            new DataGridViewTextBoxColumn { Name = "StatusText", HeaderText = "Durum", DataPropertyName = "StatusText", Width = 100 }
        );

        dgvWorkOrders.SelectionChanged += DgvWorkOrders_SelectionChanged;
        leftSplit.Panel1.Controls.Add(dgvWorkOrders);

        // Form panel
        var formPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };

        // Sol kolon - Temel Bilgiler
        var lblProduct = new Label { Text = "Ürün:", Location = new Point(10, 10), Size = new Size(120, 20), AutoSize = false };
        cmbProductForWorkOrder = new ComboBox { Location = new Point(140, 10), Size = new Size(250, 25), DropDownStyle = ComboBoxStyle.DropDownList };

        var lblWONumber = new Label { Text = "İş Emri No:", Location = new Point(10, 45), Size = new Size(120, 20), AutoSize = false };
        txtWorkOrderNumber = new TextBox { Location = new Point(140, 45), Size = new Size(150, 25) };

        var lblQuantity = new Label { Text = "Üretim Adedi:", Location = new Point(10, 80), Size = new Size(120, 20), AutoSize = false };
        numQuantity = new NumericUpDown { Location = new Point(140, 80), Size = new Size(100, 25), Maximum = 1000000, Minimum = 1, Value = 100 };

        var lblBatch = new Label { Text = "Batch No:", Location = new Point(10, 115), Size = new Size(120, 20), AutoSize = false };
        txtBatchNumber = new TextBox { Location = new Point(140, 115), Size = new Size(150, 25) };

        var lblExpDate = new Label { Text = "SKT:", Location = new Point(10, 150), Size = new Size(120, 20), AutoSize = false };
        dtpExpirationDate = new DateTimePicker { Location = new Point(140, 150), Size = new Size(150, 25), Value = DateTime.Now.AddYears(2) };

        // Sağ kolon - Agregasyon Ayarları
        var grpAggregation = new GroupBox { Text = "Agregasyon Ayarları", Location = new Point(420, 10), Size = new Size(270, 110) };
        
        var lblItemsPerBox = new Label { Text = "Koli Başına Ürün:", Location = new Point(10, 25), Size = new Size(140, 20), AutoSize = false };
        numItemsPerBox = new NumericUpDown { Location = new Point(155, 25), Size = new Size(100, 25), Maximum = 1000, Minimum = 1, Value = 10 };

        var lblBoxesPerPallet = new Label { Text = "Palet Başına Koli:", Location = new Point(10, 60), Size = new Size(140, 20), AutoSize = false };
        numBoxesPerPallet = new NumericUpDown { Location = new Point(155, 60), Size = new Size(100, 25), Maximum = 1000, Minimum = 1, Value = 100 };
        
        grpAggregation.Controls.AddRange(new Control[] { lblItemsPerBox, numItemsPerBox, lblBoxesPerPallet, numBoxesPerPallet });

        // Alt satır - Butonlar
        btnCreateWorkOrder = new Button { Text = "İş Emri Oluştur", Location = new Point(140, 190), Size = new Size(130, 30) };
        btnCreateWorkOrder.Click += BtnCreateWorkOrder_Click;

        btnRefreshWorkOrders = new Button { Text = "Yenile", Location = new Point(280, 190), Size = new Size(80, 30) };
        btnRefreshWorkOrders.Click += async (s, e) => await LoadWorkOrdersAsync();

        // Seri numara üretim
        var lblSerialCount = new Label { Text = "Seri Adet:", Location = new Point(380, 190), Size = new Size(70, 20), AutoSize = false };
        numSerialCount = new NumericUpDown { Location = new Point(455, 190), Size = new Size(80, 25), Maximum = 10000, Minimum = 1, Value = 10 };
        
        btnGenerateSerials = new Button { Text = "Seri Üret", Location = new Point(545, 190), Size = new Size(100, 30) };
        btnGenerateSerials.Click += BtnGenerateSerials_Click;

        btnViewDetails = new Button { Text = "Detay Görüntüle", Location = new Point(655, 190), Size = new Size(120, 30) };
        btnViewDetails.Click += BtnViewDetails_Click;

        formPanel.Controls.AddRange(new Control[] { lblProduct, cmbProductForWorkOrder, lblWONumber, txtWorkOrderNumber, 
            lblQuantity, numQuantity, lblBatch, txtBatchNumber, lblExpDate, dtpExpirationDate,
            grpAggregation,
            btnCreateWorkOrder, btnRefreshWorkOrders, lblSerialCount, numSerialCount, btnGenerateSerials, btnViewDetails });

        leftSplit.Panel2.Controls.Add(formPanel);
        mainSplit.Panel1.Controls.Add(leftSplit);

        // Sağ panel - Seri numaralar ve detay
        var rightSplit = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Horizontal,
            SplitterDistance = 300
        };

        dgvSerialNumbers = new DataGridView
        {
            Dock = DockStyle.Fill,
            AutoGenerateColumns = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            AllowUserToAddRows = false,
            ReadOnly = true
        };

        dgvSerialNumbers.Columns.AddRange(
            new DataGridViewTextBoxColumn { Name = "Serial", HeaderText = "Seri No", DataPropertyName = "Serial", Width = 120 },
            new DataGridViewTextBoxColumn { Name = "GS1DataMatrix", HeaderText = "GS1 Data Matrix", DataPropertyName = "GS1DataMatrix", Width = 250 },
            new DataGridViewTextBoxColumn { Name = "StatusText", HeaderText = "Durum", DataPropertyName = "StatusText", Width = 80 }
        );

        rightSplit.Panel1.Controls.Add(dgvSerialNumbers);

        rtbWorkOrderDetails = new RichTextBox { Dock = DockStyle.Fill, ReadOnly = true, Font = new Font("Consolas", 10) };
        rightSplit.Panel2.Controls.Add(rtbWorkOrderDetails);

        mainSplit.Panel2.Controls.Add(rightSplit);
        tabWorkOrders.Controls.Add(mainSplit);
    }

    private async void DgvWorkOrders_SelectionChanged(object? sender, EventArgs e)
    {
        if (dgvWorkOrders.SelectedRows.Count > 0)
        {
            var workOrder = (WorkOrderDto)dgvWorkOrders.SelectedRows[0].DataBoundItem;
            await LoadSerialNumbersAsync(workOrder.Id);
        }
    }

    private async void BtnCreateWorkOrder_Click(object? sender, EventArgs e)
    {
        if (cmbProductForWorkOrder.SelectedItem == null || string.IsNullOrWhiteSpace(txtWorkOrderNumber.Text) || string.IsNullOrWhiteSpace(txtBatchNumber.Text))
        {
            MessageBox.Show("Tüm alanlar zorunludur.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var product = (ProductDto)cmbProductForWorkOrder.SelectedItem;
        var dto = new WorkOrderCreateDto(
            txtWorkOrderNumber.Text,
            (int)numQuantity.Value,
            txtBatchNumber.Text,
            dtpExpirationDate.Value,
            1,
            product.Id,
            (int)numItemsPerBox.Value,
            (int)numBoxesPerPallet.Value
        );

        var result = await _apiClient.CreateWorkOrderAsync(dto);

        if (result?.Success == true)
        {
            MessageBox.Show("İş emri başarıyla oluşturuldu.", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
            txtWorkOrderNumber.Clear();
            txtBatchNumber.Clear();
            await LoadWorkOrdersAsync();
            await LoadWorkOrderComboAsync();
        }
        else
        {
            MessageBox.Show(result?.Message ?? "Hata oluştu", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void BtnGenerateSerials_Click(object? sender, EventArgs e)
    {
        if (dgvWorkOrders.SelectedRows.Count == 0)
        {
            MessageBox.Show("Lütfen bir iş emri seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var workOrder = (WorkOrderDto)dgvWorkOrders.SelectedRows[0].DataBoundItem;
        var result = await _apiClient.GenerateSerialsAsync(workOrder.Id, (int)numSerialCount.Value);

        if (result?.Success == true)
        {
            MessageBox.Show($"{numSerialCount.Value} adet seri numarası üretildi.", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
            await LoadSerialNumbersAsync(workOrder.Id);
        }
        else
        {
            MessageBox.Show(result?.Message ?? "Hata oluştu", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void BtnViewDetails_Click(object? sender, EventArgs e)
    {
        if (dgvWorkOrders.SelectedRows.Count == 0)
        {
            MessageBox.Show("Lütfen bir iş emri seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var workOrder = (WorkOrderDto)dgvWorkOrders.SelectedRows[0].DataBoundItem;
        var result = await _apiClient.GetWorkOrderDetailsAsync(workOrder.Id);

        if (result?.Success == true && result.Data != null)
        {
            var detail = result.Data;
            rtbWorkOrderDetails.Clear();
            rtbWorkOrderDetails.AppendText($"═══════════════════════════════════════════\n");
            rtbWorkOrderDetails.AppendText($"İŞ EMRİ DETAYI\n");
            rtbWorkOrderDetails.AppendText($"═══════════════════════════════════════════\n\n");
            rtbWorkOrderDetails.AppendText($"İş Emri No    : {detail.WorkOrderNumber}\n");
            rtbWorkOrderDetails.AppendText($"Durum         : {detail.StatusText}\n");
            rtbWorkOrderDetails.AppendText($"Üretim Adedi  : {detail.ProductionQuantity}\n");
            rtbWorkOrderDetails.AppendText($"Batch No      : {detail.BatchNumber}\n");
            rtbWorkOrderDetails.AppendText($"SKT           : {detail.ExpirationDate:yyyy-MM-dd}\n\n");
            
            rtbWorkOrderDetails.AppendText($"─── ÜRÜN BİLGİLERİ ───\n");
            rtbWorkOrderDetails.AppendText($"Ürün Adı      : {detail.Product.ProductName}\n");
            rtbWorkOrderDetails.AppendText($"GTIN (01)     : {detail.Product.GTIN}\n\n");
            
            rtbWorkOrderDetails.AppendText($"─── MÜŞTERİ BİLGİLERİ ───\n");
            rtbWorkOrderDetails.AppendText($"Firma         : {detail.Customer.CompanyName}\n");
            rtbWorkOrderDetails.AppendText($"GLN           : {detail.Customer.GLN}\n\n");
            
            rtbWorkOrderDetails.AppendText($"─── ÖZET ───\n");
            rtbWorkOrderDetails.AppendText($"Toplam Seri   : {detail.Summary.TotalSerialNumbers}\n");
            rtbWorkOrderDetails.AppendText($"Basılan       : {detail.Summary.PrintedCount}\n");
            rtbWorkOrderDetails.AppendText($"Doğrulanan    : {detail.Summary.VerifiedCount}\n");
            rtbWorkOrderDetails.AppendText($"Reddedilen    : {detail.Summary.RejectedCount}\n");
            rtbWorkOrderDetails.AppendText($"Agregeli      : {detail.Summary.AggregatedCount}\n");
            rtbWorkOrderDetails.AppendText($"Koli Sayısı   : {detail.Summary.TotalBoxes}\n");
            rtbWorkOrderDetails.AppendText($"Palet Sayısı  : {detail.Summary.TotalPallets}\n");
            rtbWorkOrderDetails.AppendText($"İlerleme      : %{detail.Summary.CompletionPercentage}\n\n");
            
            if (detail.SerialNumbers.Any())
            {
                rtbWorkOrderDetails.AppendText($"─── GS1 DATA MATRIX ÖRNEKLERİ ───\n");
                foreach (var sn in detail.SerialNumbers.Take(5))
                {
                    rtbWorkOrderDetails.AppendText($"(21){sn.Serial} → {sn.GS1DataMatrix}\n");
                }
                if (detail.SerialNumbers.Count > 5)
                    rtbWorkOrderDetails.AppendText($"... ve {detail.SerialNumbers.Count - 5} adet daha\n");
            }
        }
        else
        {
            MessageBox.Show(result?.Message ?? "Hata oluştu", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task LoadWorkOrdersAsync()
    {
        var result = await _apiClient.GetWorkOrdersAsync();
        if (result?.Success == true && result.Data != null)
        {
            dgvWorkOrders.DataSource = result.Data.ToList();
        }
        await LoadProductComboAsync();
    }

    private async Task LoadProductComboAsync()
    {
        var result = await _apiClient.GetProductsAsync();
        if (result?.Success == true && result.Data != null)
        {
            cmbProductForWorkOrder.DataSource = result.Data.ToList();
            cmbProductForWorkOrder.DisplayMember = "ProductName";
            cmbProductForWorkOrder.ValueMember = "Id";
        }
    }

    private async Task LoadSerialNumbersAsync(Guid workOrderId)
    {
        var result = await _apiClient.GetSerialNumbersByWorkOrderAsync(workOrderId);
        if (result?.Success == true && result.Data != null)
        {
            dgvSerialNumbers.DataSource = result.Data.ToList();
        }
    }

    #endregion

    #region Aggregation Tab

    private void CreateAggregationTab()
    {
        tabAggregation = new TabPage("Agregasyon");

        var mainSplit = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Vertical,
            SplitterDistance = 700
        };

        // Sol panel
        var leftPanel = new Panel { Dock = DockStyle.Fill };

        var lblSelectWO = new Label { Text = "İş Emri:", Location = new Point(10, 10), Size = new Size(60, 20) };
        cmbWorkOrderForAggregation = new ComboBox { Location = new Point(80, 10), Size = new Size(300, 25), DropDownStyle = ComboBoxStyle.DropDownList };
        cmbWorkOrderForAggregation.SelectedIndexChanged += CmbWorkOrderForAggregation_SelectedIndexChanged;

        btnCreateBox = new Button { Text = "Yeni Koli", Location = new Point(400, 8), Size = new Size(100, 28) };
        btnCreateBox.Click += BtnCreateBox_Click;

        btnCreatePallet = new Button { Text = "Yeni Palet", Location = new Point(510, 8), Size = new Size(100, 28) };
        btnCreatePallet.Click += BtnCreatePallet_Click;

        // Atanmamış ürünler
        var lblUnassigned = new Label { Text = "Atanmamış Ürünler:", Location = new Point(10, 50), Size = new Size(150, 20) };
        dgvUnassignedItems = new DataGridView
        {
            Location = new Point(10, 75),
            Size = new Size(300, 200),
            AutoGenerateColumns = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            AllowUserToAddRows = false,
            ReadOnly = false,
            MultiSelect = true
        };
        dgvUnassignedItems.Columns.AddRange(
            new DataGridViewCheckBoxColumn { Name = "Select", HeaderText = "", Width = 30, ReadOnly = false },
            new DataGridViewTextBoxColumn { Name = "Id", HeaderText = "ID", DataPropertyName = "Id", Width = 50, ReadOnly = true },
            new DataGridViewTextBoxColumn { Name = "Serial", HeaderText = "Seri No", DataPropertyName = "Serial", Width = 120, ReadOnly = true }
        );
        dgvUnassignedItems.CellContentClick += DgvUnassignedItems_CellContentClick;

        btnAddToBox = new Button { Text = "Seçilenleri Koliye Ekle →", Location = new Point(10, 285), Size = new Size(180, 30) };
        btnAddToBox.Click += BtnAddToBox_Click;

        // Koliler
        var lblBoxes = new Label { Text = "Koliler:", Location = new Point(330, 50), Size = new Size(100, 20) };
        dgvBoxes = new DataGridView
        {
            Location = new Point(330, 75),
            Size = new Size(330, 150),
            AutoGenerateColumns = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            AllowUserToAddRows = false,
            ReadOnly = false,
            MultiSelect = true
        };
        dgvBoxes.Columns.AddRange(
            new DataGridViewCheckBoxColumn { Name = "Select", HeaderText = "", Width = 30, ReadOnly = false },
            new DataGridViewTextBoxColumn { Name = "Id", HeaderText = "ID", DataPropertyName = "Id", Width = 40, ReadOnly = true },
            new DataGridViewTextBoxColumn { Name = "SSCCCode", HeaderText = "SSCC", DataPropertyName = "SSCCCode", Width = 120, ReadOnly = true },
            new DataGridViewTextBoxColumn { Name = "ItemCount", HeaderText = "Ürün", DataPropertyName = "ItemCount", Width = 45, ReadOnly = true },
            new DataGridViewTextBoxColumn { Name = "ItemsPerBox", HeaderText = "Kapasite", DataPropertyName = "ItemsPerBox", Width = 60, ReadOnly = true }
        );
        dgvBoxes.CellContentClick += DgvBoxes_CellContentClick;

        btnAddToPallet = new Button { Text = "Seçilenleri Palete Ekle ↓", Location = new Point(330, 235), Size = new Size(180, 30) };
        btnAddToPallet.Click += BtnAddToPallet_Click;

        // Paletler
        var lblPallets = new Label { Text = "Paletler:", Location = new Point(330, 275), Size = new Size(100, 20) };
        dgvPallets = new DataGridView
        {
            Location = new Point(330, 300),
            Size = new Size(330, 150),
            AutoGenerateColumns = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            AllowUserToAddRows = false,
            ReadOnly = true
        };
        dgvPallets.Columns.AddRange(
            new DataGridViewTextBoxColumn { Name = "Id", HeaderText = "ID", DataPropertyName = "Id", Width = 50 },
            new DataGridViewTextBoxColumn { Name = "SSCCCode", HeaderText = "SSCC", DataPropertyName = "SSCCCode", Width = 120 },
            new DataGridViewTextBoxColumn { Name = "ItemCount", HeaderText = "Koli", DataPropertyName = "ItemCount", Width = 50 },
            new DataGridViewButtonColumn { Name = "Details", HeaderText = "Detay", Text = "Göster", UseColumnTextForButtonValue = true, Width = 60 }
        );
        dgvPallets.CellClick += DgvPallets_CellClick;

        leftPanel.Controls.AddRange(new Control[] { lblSelectWO, cmbWorkOrderForAggregation, btnCreateBox, btnCreatePallet,
            lblUnassigned, dgvUnassignedItems, btnAddToBox, lblBoxes, dgvBoxes, btnAddToPallet, lblPallets, dgvPallets });

        mainSplit.Panel1.Controls.Add(leftPanel);

        // Sağ panel - Hiyerarşi
        var rightPanel = new Panel { Dock = DockStyle.Fill };
        var lblHierarchy = new Label { Text = "Agregasyon Hiyerarşisi:", Location = new Point(10, 10), Size = new Size(200, 20) };
        tvAggregationHierarchy = new TreeView { Location = new Point(10, 35), Size = new Size(350, 400) };
        
        rightPanel.Controls.AddRange(new Control[] { lblHierarchy, tvAggregationHierarchy });
        mainSplit.Panel2.Controls.Add(rightPanel);

        tabAggregation.Controls.Add(mainSplit);
    }

    private async void CmbWorkOrderForAggregation_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (cmbWorkOrderForAggregation.SelectedItem is WorkOrderDto workOrder)
        {
            await LoadAggregationDataAsync(workOrder.Id);
        }
    }

    private async void BtnCreateBox_Click(object? sender, EventArgs e)
    {
        if (cmbWorkOrderForAggregation.SelectedItem is WorkOrderDto workOrder)
        {
            var result = await _apiClient.CreateBoxAsync(workOrder.Id);
            if (result?.Success == true)
            {
                MessageBox.Show($"Koli oluşturuldu: {result.Data?.SSCCCode}", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await LoadAggregationDataAsync(workOrder.Id);
            }
            else
            {
                MessageBox.Show(result?.Message ?? "Hata", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    private async void BtnCreatePallet_Click(object? sender, EventArgs e)
    {
        if (cmbWorkOrderForAggregation.SelectedItem is WorkOrderDto workOrder)
        {
            var result = await _apiClient.CreatePalletAsync(workOrder.Id);
            if (result?.Success == true)
            {
                MessageBox.Show($"Palet oluşturuldu: {result.Data?.SSCCCode}", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await LoadAggregationDataAsync(workOrder.Id);
            }
            else
            {
                MessageBox.Show(result?.Message ?? "Hata", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    private async void BtnAddToBox_Click(object? sender, EventArgs e)
    {
        if (dgvBoxes.SelectedRows.Count == 0)
        {
            MessageBox.Show("Lütfen bir koli seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var selectedSerials = new List<Guid>();
        foreach (DataGridViewRow row in dgvUnassignedItems.Rows)
        {
            if (row.Cells["Select"].Value is true)
            {
                var serial = (SerialNumberDto)row.DataBoundItem;
                selectedSerials.Add(serial.Id);
            }
        }

        if (selectedSerials.Count == 0)
        {
            MessageBox.Show("Lütfen eklenecek ürünleri seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var box = (SSCCDto)dgvBoxes.SelectedRows[0].DataBoundItem;
        var result = await _apiClient.AddItemsToBoxAsync(box.Id, selectedSerials);

        if (result?.Success == true)
        {
            MessageBox.Show($"{selectedSerials.Count} ürün koliye eklendi.", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
            var workOrder = (WorkOrderDto)cmbWorkOrderForAggregation.SelectedItem!;
            await LoadAggregationDataAsync(workOrder.Id);
        }
        else
        {
            MessageBox.Show(result?.Message ?? "Hata", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void BtnAddToPallet_Click(object? sender, EventArgs e)
    {
        if (dgvPallets.SelectedRows.Count == 0)
        {
            MessageBox.Show("Lütfen bir palet seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var selectedBoxes = new List<Guid>();
        foreach (DataGridViewRow row in dgvBoxes.Rows)
        {
            if (row.Cells["Select"].Value is true)
            {
                var box = (SSCCDto)row.DataBoundItem;
                selectedBoxes.Add(box.Id);
            }
        }

        if (selectedBoxes.Count == 0)
        {
            MessageBox.Show("Lütfen palete eklenecek kolileri seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var pallet = (SSCCDto)dgvPallets.SelectedRows[0].DataBoundItem;
        var result = await _apiClient.AddBoxesToPalletAsync(pallet.Id, selectedBoxes);

        if (result?.Success == true)
        {
            MessageBox.Show($"{selectedBoxes.Count} koli palete eklendi.", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
            var workOrder = (WorkOrderDto)cmbWorkOrderForAggregation.SelectedItem!;
            await LoadAggregationDataAsync(workOrder.Id);
        }
        else
        {
            MessageBox.Show(result?.Message ?? "Hata", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task LoadAggregationDataAsync(Guid workOrderId)
    {
        // Atanmamış ürünler
        var unassignedResult = await _apiClient.GetUnassignedSerialsAsync(workOrderId);
        if (unassignedResult?.Success == true && unassignedResult.Data != null)
        {
            dgvUnassignedItems.DataSource = unassignedResult.Data.ToList();
        }

        // Koliler
        var boxesResult = await _apiClient.GetBoxesAsync(workOrderId);
        if (boxesResult?.Success == true && boxesResult.Data != null)
        {
            dgvBoxes.DataSource = boxesResult.Data.ToList();
        }

        // Paletler
        var palletsResult = await _apiClient.GetPalletsAsync(workOrderId);
        if (palletsResult?.Success == true && palletsResult.Data != null)
        {
            dgvPallets.DataSource = palletsResult.Data.ToList();
        }

        // Hiyerarşi ağacı
        await BuildAggregationTreeAsync(workOrderId);
    }

    private async Task BuildAggregationTreeAsync(Guid workOrderId)
    {
        tvAggregationHierarchy.Nodes.Clear();

        var palletsResult = await _apiClient.GetPalletsAsync(workOrderId);

        if (palletsResult?.Success == true && palletsResult.Data != null)
        {
            foreach (var pallet in palletsResult.Data)
            {
                // Her palet için detaylı hiyerarşi bilgisini getir
                var hierarchyResult = await _apiClient.GetAggregationHierarchyAsync(pallet.Id);
                
                if (hierarchyResult?.Success == true && hierarchyResult.Data != null)
                {
                    var palletData = hierarchyResult.Data;
                    var palletNode = new TreeNode($"🎨 Palet: {palletData.SSCCCode} ({palletData.ChildBoxes?.Count ?? 0} koli)");
                    palletNode.Tag = palletData;
                    
                    // Palete bağlı kolileri ekle
                    if (palletData.ChildBoxes != null)
                    {
                        foreach (var box in palletData.ChildBoxes)
                        {
                            var boxNode = new TreeNode($"📦 Koli: {box.SSCCCode} ({box.Items?.Count ?? 0} ürün)");
                            boxNode.Tag = box;
                            
                            // Kolideki ürünleri ekle
                            if (box.Items != null)
                            {
                                foreach (var item in box.Items)
                                {
                                    var itemNode = new TreeNode($"🏷️ Seri: {item.Serial} - {item.StatusText}");
                                    itemNode.Tag = item;
                                    boxNode.Nodes.Add(itemNode);
                                }
                            }
                            
                            palletNode.Nodes.Add(boxNode);
                        }
                    }
                    
                    tvAggregationHierarchy.Nodes.Add(palletNode);
                }
            }
        }

        // Palete bağlı olmayan kolileri göster
        var boxesResult = await _apiClient.GetBoxesAsync(workOrderId);
        if (boxesResult?.Success == true && boxesResult.Data != null)
        {
            foreach (var box in boxesResult.Data.Where(b => b.ParentSSCCId == null))
            {
                // Her koli için detaylı bilgi getir
                var hierarchyResult = await _apiClient.GetAggregationHierarchyAsync(box.Id);
                
                if (hierarchyResult?.Success == true && hierarchyResult.Data != null)
                {
                    var boxData = hierarchyResult.Data;
                    var boxNode = new TreeNode($"📦 Koli: {boxData.SSCCCode} ({boxData.Items?.Count ?? 0} ürün)");
                    boxNode.Tag = boxData;
                    
                    // Kolideki ürünleri ekle
                    if (boxData.Items != null)
                    {
                        foreach (var item in boxData.Items)
                        {
                            var itemNode = new TreeNode($"🏷️ Seri: {item.Serial} - {item.StatusText}");
                            itemNode.Tag = item;
                            boxNode.Nodes.Add(itemNode);
                        }
                    }
                    
                    tvAggregationHierarchy.Nodes.Add(boxNode);
                }
            }
        }

        tvAggregationHierarchy.ExpandAll();
    }

    private async Task LoadWorkOrderComboAsync()
    {
        var result = await _apiClient.GetWorkOrdersAsync();
        if (result?.Success == true && result.Data != null)
        {
            cmbWorkOrderForAggregation.DataSource = result.Data.ToList();
            cmbWorkOrderForAggregation.DisplayMember = "WorkOrderNumber";
            cmbWorkOrderForAggregation.ValueMember = "Id";
        }
    }

    private void DgvUnassignedItems_CellContentClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.ColumnIndex == 0 && e.RowIndex >= 0) // Select column
        {
            dgvUnassignedItems.CommitEdit(DataGridViewDataErrorContexts.Commit);
        }
    }

    private void DgvBoxes_CellContentClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.ColumnIndex == 0 && e.RowIndex >= 0) // Select column
        {
            dgvBoxes.CommitEdit(DataGridViewDataErrorContexts.Commit);
        }
    }

    private async void DgvPallets_CellClick(object? sender, DataGridViewCellEventArgs e)
    {
        // Detay butonu kolonuna tıklanmışsa
        if (e.RowIndex < 0 || e.ColumnIndex != dgvPallets.Columns["Details"].Index) return;

        try
        {
            var pallet = (SSCCDto)dgvPallets.Rows[e.RowIndex].DataBoundItem;
            var result = await _apiClient.GetAggregationHierarchyAsync(pallet.Id);

            if (result?.Success == true && result.Data != null)
            {
                ShowPalletDetailsDialog(result.Data);
            }
            else
            {
                MessageBox.Show(result?.Message ?? "Palet detayları yüklenemedi", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Hata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ShowPalletDetailsDialog(SSCCAggregationDto pallet)
    {
        var dialog = new Form
        {
            Text = $"Palet Detayları - {pallet.SSCCCode}",
            Size = new Size(800, 600),
            StartPosition = FormStartPosition.CenterParent,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false,
            MinimizeBox = false
        };

        var lblPalletInfo = new Label
        {
            Text = $"SSCC: {pallet.SSCCCode}\nToplam Koli: {pallet.ChildBoxes?.Count ?? 0}",
            Location = new Point(10, 10),
            Size = new Size(760, 40),
            Font = new Font(Font.FontFamily, 10, FontStyle.Bold)
        };

        var lblBoxes = new Label
        {
            Text = "Koliler:",
            Location = new Point(10, 60),
            Size = new Size(100, 20)
        };

        var dgvBoxDetails = new DataGridView
        {
            Location = new Point(10, 85),
            Size = new Size(760, 200),
            AutoGenerateColumns = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            AllowUserToAddRows = false,
            ReadOnly = true
        };

        dgvBoxDetails.Columns.AddRange(
            new DataGridViewTextBoxColumn { Name = "SSCCCode", HeaderText = "SSCC Kodu", DataPropertyName = "SSCCCode", Width = 200 },
            new DataGridViewTextBoxColumn { Name = "ItemCount", HeaderText = "Ürün Sayısı", DataPropertyName = "ItemCount", Width = 100 }
        );

        if (pallet.ChildBoxes != null && pallet.ChildBoxes.Any())
        {
            var boxList = pallet.ChildBoxes.Select(b => new
            {
                b.SSCCCode,
                ItemCount = b.Items?.Count ?? 0,
                Items = b.Items
            }).ToList();

            dgvBoxDetails.DataSource = boxList;

            // Koli seçildiğinde ürünleri göster
            var lblItems = new Label
            {
                Text = "Seçili Kolideki Ürünler:",
                Location = new Point(10, 295),
                Size = new Size(200, 20)
            };

            var dgvItems = new DataGridView
            {
                Location = new Point(10, 320),
                Size = new Size(760, 200),
                AutoGenerateColumns = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AllowUserToAddRows = false,
                ReadOnly = true
            };

            dgvItems.Columns.AddRange(
                new DataGridViewTextBoxColumn { Name = "Serial", HeaderText = "Seri No", DataPropertyName = "Serial", Width = 150 },
                new DataGridViewTextBoxColumn { Name = "GS1DataMatrix", HeaderText = "GS1 DataMatrix", DataPropertyName = "GS1DataMatrix", Width = 400 },
                new DataGridViewTextBoxColumn { Name = "StatusText", HeaderText = "Durum", DataPropertyName = "StatusText", Width = 100 }
            );

            dgvBoxDetails.SelectionChanged += (s, e) =>
            {
                if (dgvBoxDetails.SelectedRows.Count > 0)
                {
                    var selectedBox = dgvBoxDetails.SelectedRows[0].DataBoundItem as dynamic;
                    if (selectedBox?.Items != null)
                    {
                        dgvItems.DataSource = selectedBox.Items;
                    }
                }
            };

            dialog.Controls.AddRange(new Control[] { lblPalletInfo, lblBoxes, dgvBoxDetails, lblItems, dgvItems });
        }
        else
        {
            var lblEmpty = new Label
            {
                Text = "Bu palette henüz koli bulunmuyor.",
                Location = new Point(10, 110),
                Size = new Size(760, 20),
                ForeColor = Color.Gray
            };
            dialog.Controls.AddRange(new Control[] { lblPalletInfo, lblBoxes, dgvBoxDetails, lblEmpty });
        }

        var btnClose = new Button
        {
            Text = "Kapat",
            Location = new Point(690, 530),
            Size = new Size(80, 30)
        };
        btnClose.Click += (s, e) => dialog.Close();
        dialog.Controls.Add(btnClose);

        dialog.ShowDialog();
    }

    #endregion
}
