resource "hcloud_volume" "this" {
  name     = var.volume_name
  size     = var.size_gb
  location = var.location

  lifecycle {
    prevent_destroy = true
  }
}