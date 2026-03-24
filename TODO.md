Cloudflare ddns:

- create a method to get the machines current ip - how would you do it without relying on external services? - who do i trust - how do i do it securely? - implemented using cloudflare trace. needs redoing.
- read config file json for api key and what domain / subdomain a records to update.
- send get request to the correct Zone id to get the current ip for the wanted record
- check if the ip i got returned from the ip check is valid.
- if the ip is valid and the ip is different from the cloudflare a record run update method - use update call not overwrite 
- create update method to send post request to update the correct subdomain for the correct zone
- make this run in a infinite loop with a configurable delay between ip change checks.
