plugin "terraform" {
  enabled = true
  preset  = "recommended"
}

plugin "hcloud" {
  enabled = true
  version = "0.1.0"
  source  = "github.com/hetznercloud/terraform-provider-hcloud"
}