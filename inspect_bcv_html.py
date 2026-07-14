import urllib.request
import re

url = 'https://www.bcv.org.ve/'
req = urllib.request.Request(url, headers={'User-Agent': 'Mozilla/5.0'})
html = urllib.request.urlopen(req, timeout=30).read().decode('utf-8', errors='replace')
print('LENGTH', len(html))
for term in ['USD', 'EUR', 'DÓLAR', 'DOLAR', 'EURO', 'tasa', 'cambio', 'compra', 'venta']:
    print('\n===', term, '===')
    for m in re.finditer(r'.{0,120}' + re.escape(term) + r'.{0,120}', html, flags=re.I):
        s = m.group(0).replace('\n', ' ').replace('\r', ' ')
        print(s)
        print('---')

# Print a short snippet around the first occurrence of 'tipo de cambio'
match = re.search(r'.{0,200}tipo de cambio.{0,200}', html, flags=re.I)
if match:
    snippet = match.group(0).replace('\n', ' ').replace('\r', ' ')
    print('\n=== tipo de cambio snippet ===')
    print(snippet)
