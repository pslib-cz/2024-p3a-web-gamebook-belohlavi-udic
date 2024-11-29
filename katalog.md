# Katalog požadavků

## ⚙️ **Technická specifikace**

### **1. Backend API**
- **Účel:**  
  - Poskytuje data o herním světě (místnosti, přechody, popisy).  
  - Sleduje dynamický stav hráče (HP, aktuální poloha, postup).  

- **Hlavní endpointy:**  
  - `/rooms` – seznam místností a jejich vlastností.  
  - `/player` – informace o aktuálním stavu hráče.  
  - `/game-state` – ukládání a načítání průběhu hry.  

### **2. Frontend aplikace**
- **Funkce:**  
  - Zobrazení popisu prostředí (textové zpracování).  
  - Možnost výběru akcí (procházení místnostmi, interakce s prostředím).  

- **Technologie:**  
  - React pro správu stavu aplikace.  
  - Stylizace odpovídající atmosféře survival adventury.  

### **3. Databáze**
- **Struktura:**  
  - **Permanentní data:** Definice místností, přechodů, pastí a událostí.  
  - **Dynamická data:** Stav hry a hráče, ukládání postupu.  

- **Možnosti implementace:**  
  - **Serverová databáze:** Pro sdílený přístup odkudkoliv. (SQLite)
  - **Lokální úložiště:** Data jsou ukládána pouze na zařízení hráče.
