import pandas as pd
import requests

# Đường dẫn file Excel
file_path = "/Users/thanhbinh/Downloads/unit.xlsx"

# Đọc file Excel, bỏ dòng tiêu đề (dòng đầu tiên)
df = pd.read_excel(file_path, header=None, engine='openpyxl')
df = df.iloc[1:]

# API endpoint và token
url = "http://localhost:5093/api/Material"
token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiaCIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWVpZGVudGlmaWVyIjoiMSIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IjEiLCJleHAiOjE3NTQyOTI3MjEsImlzcyI6InlvdXJhcHAiLCJhdWQiOiJ5b3VyYXBwX3VzZXJzIn0.9fesRkAIeBvmi8SxuUQiKdcWkZCQUcaxIUbh0YBUCwU"

headers = {
    "Authorization": f"Bearer {token}",
    "Content-Type": "application/json"
}

# Gửi từng hàng
for index, row in df.iterrows():
    name = row[0]
    description = row[1]
    unit_id = row[3]

    payload = {
        "name": str(name) if not pd.isna(name) else "",
        "isActive": True,
        "description": str(description) if not pd.isna(description) else "",
        "createdById": 1
    }

    if not pd.isna(unit_id):
        payload["unitId"] = int(unit_id)

    try:
        response = requests.post(url, json=payload, headers=headers)
        if response.status_code in [200, 201]:
            print(f"[✓] Row {index + 2} OK")
        else:
            print(f"[✗] Row {index + 2} FAILED | Status: {response.status_code} | Response: {response.text}")
    except Exception as e:
        print(f"[!] Row {index + 2} ERROR | Exception: {e}")
