import pandas as pd
import requests

# File Excel đầu vào
file_path = "/Users/thanhbinh/Downloads/employee.xlsx"

# Đọc file, không dùng header, bỏ dòng tiêu đề (dòng 1)
df = pd.read_excel(file_path, header=None, engine='openpyxl')
df = df.iloc[1:]  # Bỏ dòng đầu tiên (tiêu đề)

# API endpoint & headers
url = "http://localhost:5093/api/Customer"
token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiaCIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWVpZGVudGlmaWVyIjoiMSIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IjEiLCJleHAiOjE3NTQyOTI3MjEsImlzcyI6InlvdXJhcHAiLCJhdWQiOiJ5b3VyYXBwX3VzZXJzIn0.9fesRkAIeBvmi8SxuUQiKdcWkZCQUcaxIUbh0YBUCwU"

headers = {
    "Authorization": f"Bearer {token}",
    "Content-Type": "application/json"
}

# Gửi từng dòng
for index, row in df.iterrows():
    code = row[0]
    name = row[1]
    address = row[2]

    payload = {
        "code": str(code) if not pd.isna(code) else "1",
        "name": str(name) if not pd.isna(name) else "1",
        "address": str(address) if not pd.isna(address) else "",
        "isActive": True
    }

    try:
        response = requests.post(url, json=payload, headers=headers)
        if response.status_code in [200, 201]:
            print(f"[✓] Row {index + 2} OK")
        else:
            print(f"[✗] Row {index + 2} FAILED | Status: {response.status_code} | Response: {response.text}")
    except Exception as e:
        print(f"[!] Row {index + 2} ERROR | Exception: {e}")
