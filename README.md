# Quran Bangla Text Extractor

## 📖 Background
This project provides a way to extract and use **Bangla translations of the Quran** without directly handling the Arabic text.

- Many scholars strongly discourage touching the **original Arabic Quran text** without *wudu* (ablution), even on a mobile screen.  
- Scholars also discourage reading any Quran translation where the authors have little or no proper Islamic knowledge.  
- Although there are many Qurans available with Bangla translations, some editions are **too small**, **too detailed with tafsir**, or **only available in hard copy**, making them inconvenient for my regular daily use.
- For personal usage, this tool focuses on **Bangla-only translations**, making it easier to read and carry everywhere while respecting those scholarly perspectives.  
- This project uses a public API that provides **authentic translations from world-renowned scholar Mufti Taqi Usmani**.

The data is fetched from the public API of [muslimbangla.com](https://muslimbangla.com/quran).

---

## ⚙️ Tools & Frameworks
- **.NET 9** → Backend framework  
- **QuestPDF** → PDF generation (with Companion Mode for preview)  
- **SQLite** → Lightweight database storage  

---

## 🖥️ Application Features
This is a **console application** with 3 main options:  

1. **Extract** → Fetches Bangla Quran data from the Muslim Bangla API and stores it in SQLite.  
2. **Preview PDF** → Uses QuestPDF’s Companion Mode to preview layout with sample data.  
3. **Generate PDF** → Creates a full Bangla Quran translation PDF from extracted data.  

---

## 🚀 Run Procedure
1. Apply database migrations:  
   ```bash
   dotnet ef database update
2. Run the application (e.g., F5 in IDE or:
    ```bash
   dotnet run
3. Use the Extract option to fetch all data.
4. Run again → choose Preview PDF to check layout with sample data.
5. Run again → choose Generate PDF to build the final Quran Bangla translation PDF.

## 📄 Reference Output
For reference, a sample **generated Quran PDF** is also attached.  
Please note that when you generate a new PDF, it will be saved automatically inside the project’s **`bin` folder`**.

---

---
## 📌 Notes
- This tool is for personal use, respecting scholarly concerns while enabling easier access to Bangla Quran translations. 
- All content comes from a public API at [muslimbangla.com](https://muslimbangla.com/quran).
