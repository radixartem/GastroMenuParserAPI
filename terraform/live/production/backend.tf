terraform {
  backend "s3" {
    bucket   = "terraform-state"
    key      = "gastro-api/production/terraform.tfstate"
    region   = "eu-central-1"
    endpoint = "https://your-fsn1.your-objectstorage.com"   # замените
    skip_credentials_validation = true
    skip_region_validation      = true
    skip_metadata_api_check     = true
    # access_key и secret_key passed through environment variables
  }
}