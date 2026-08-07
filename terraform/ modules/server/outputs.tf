output "server_id" {
  value = hcloud_server.this.id
}

output "ipv4_address" {
  value = hcloud_server.this.ipv4_address
}

output "server_name" {
  value = hcloud_server.this.name
}