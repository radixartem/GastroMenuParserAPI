output "network_id" {
  value = hcloud_network.this.id
}

output "network_ip_range" {
  value = hcloud_network.this.ip_range
}