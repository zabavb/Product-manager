using Azure;
using Azure.Data.Tables;
using Azure.Storage.Blobs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace Exam_first_task_csh
{
    public partial class Form1 : Form
    {
        private const string BlobConStr = "DefaultEndpointsProtocol=https;AccountName=[...];AccountKey=[...core.windows.net]";
        private const string TableConStr = "DefaultEndpointsProtocol=https;AccountName=[...];AccountKey=[...core.windows.net]";
        private const string ConName = "productphotos";

        public Form1()
        {
            InitializeComponent();
            btnInsert.Click += async (sender, e) => await btnInsert_Click(sender, e);
            btnUpdate.Click += async (sender, e) => await btnUpdate_Click(sender, e);
            btnDelete.Click += async (sender, e) => await btnDelete_Click(sender, e);
            btnSelect.Click += async (sender, e) => await btnSelect_Click(sender, e);
        }

        public async Task<Product> SELECT_ProductAsync(string partitionKey, string rowKey)
        {
            TableServiceClient tableSrvCl = new TableServiceClient(TableConStr);
            TableClient tableCl = tableSrvCl.GetTableClient("Product");

            Response<Product> product = await tableCl.GetEntityAsync<Product>(partitionKey, rowKey);

            return product;
        }
        private async Task btnSelect_Click(object sender, EventArgs e)
        {
            Product product = await SELECT_ProductAsync("Product", txtId.Text);
            if (product == null)
            {
                MessageBox.Show("Not found");
            }
            else
            {
                txtName.Text = product.Name;
                txtPrice.Text = product.Price.ToString();
                txtDescription.Text = product.Description;
                txtPath.Text = product.ImgUrl;

                string imgUrl = product.ImgUrl.Trim();
                pictureBox.LoadAsync(imgUrl);

                MessageBox.Show("Success");
            }
        }

        public async Task<string> FS_ImgAsync(string path)
        {
            BlobServiceClient blobSrvCl = new BlobServiceClient(BlobConStr);
            BlobContainerClient blobConCl = blobSrvCl.GetBlobContainerClient(ConName);

            await blobConCl.CreateIfNotExistsAsync();
            BlobClient blobCl = blobConCl.GetBlobClient(Path.GetFileName(path));

            using (FileStream fs = File.OpenRead(path))
                await blobCl.UploadAsync(fs, true);

            return blobCl.Uri.ToString();
        }
        private async void btnBrowse_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.png, *.jpg) | *.png; *.jpg";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                txtPath.Text = openFileDialog.FileName;
                pictureBox.Image = Image.FromFile(openFileDialog.FileName);
                pictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
            }
        }

        public async Task INSERT_ProductAsync(Product product)
        {
            TableServiceClient tableSrvCl = new TableServiceClient(TableConStr);
            TableClient tableCl = tableSrvCl.GetTableClient("Product");

            await tableCl.CreateIfNotExistsAsync();

            BlobServiceClient blobSrvCl = new BlobServiceClient(BlobConStr);
            BlobContainerClient containerCl = blobSrvCl.GetBlobContainerClient(ConName);
            BlobClient blobCl = containerCl.GetBlobClient(Path.GetFileName(product.ImgUrl));
            product.ImgBlobId = blobCl.Name;

            await tableCl.AddEntityAsync(product);
        }
        private async Task btnInsert_Click(object sender, EventArgs e)
        {
            string blobUrl = await FS_ImgAsync(txtPath.Text);
            Product product = new Product(txtId.Text, txtName.Text, double.Parse(txtPrice.Text), txtDescription.Text, blobUrl);
            await INSERT_ProductAsync(product);
            MessageBox.Show("Success");
        }

        public async Task UPDATE_ProductAsync(Product product)
        {
            TableServiceClient tableSrvCl = new TableServiceClient(TableConStr);
            TableClient tableCl = tableSrvCl.GetTableClient("Product");

            await tableCl.UpdateEntityAsync(product, ETag.All, TableUpdateMode.Replace);
        }
        private async Task btnUpdate_Click(object sender, EventArgs e)
        {
            Product product = await SELECT_ProductAsync("Product", txtId.Text);
            if (product != null)
            {
                bool dataChanged = false;

                if (!string.IsNullOrEmpty(txtName.Text))
                {
                    product.Name = txtName.Text;
                    dataChanged = true;
                }
                if (!string.IsNullOrEmpty(txtPrice.Text))
                {
                    product.Price = int.Parse(txtPrice.Text);
                    dataChanged = true;
                }
                if (!string.IsNullOrEmpty(txtDescription.Text))
                {
                    product.Description = txtDescription.Text;
                    dataChanged = true;
                }
                if (dataChanged)
                {
                    await UPDATE_ProductAsync(product);
                    MessageBox.Show("Success");
                }
            }
            else
                MessageBox.Show("Not found");
        }

        public async Task DELETE_ImgAsync(string blobNam)
        {
            BlobServiceClient blobSrvCl = new BlobServiceClient(BlobConStr);
            BlobContainerClient blobConCl = blobSrvCl.GetBlobContainerClient(ConName);

            BlobClient blobCl = blobConCl.GetBlobClient(blobNam);

            await blobCl.DeleteAsync();
        }
        public async Task DELETE_ProductAsync(string partitionKey, string rowKey)
        {
            TableServiceClient tableSrvCl = new TableServiceClient(TableConStr);
            TableClient tableCl = tableSrvCl.GetTableClient("Product");

            await tableCl.DeleteEntityAsync(partitionKey, rowKey);
        }
        private async Task btnDelete_Click(object sender, EventArgs e)
        {
            await DELETE_ProductAsync("Product", txtId.Text);

            Product product = await SELECT_ProductAsync("Product", txtId.Text);
            if (product == null)
            {
                MessageBox.Show("Not found");
            }
            else
            {
                await DELETE_ImgAsync(product.ImgBlobId);
            }
        }
    }
}
