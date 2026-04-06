"1. Giriş",
"Amaç: BIST verilerini kullanarak hem sayısal tahmin hem de genel sorulara yanıt verebilen AI agent geliştirmek.",
"Kullanılan teknolojiler: .NET / C#, ML.NET, Semantic Kernel, Qdrant / Vector DB",

"2. Agent ve Veri Yapısı",
"2.1 Veri Kaynağı: BIST geçmiş verileri: Open, High, Low, Close, Volume",
"Veri saklama: PostgreSQL (ML.NET için), Vector DB (LLM için)",
"2.2 Agent Mantığı: Kullanıcı sorusunu alır, soruyu analiz eder, tahmin veya genel bilgi yöntemi seçilir.",

"3. ML.NET Tahmin Modeli",
"Features: Open, High, Low, Volume",
"Trainer: FastTree (regression)",
"Tahmin: Son günün verisi üzerinden kapanış fiyatı öngörülür.",

"4. Qdrant + LLM (RAG)",
"Vector DB: embedding olarak veri saklanır",
"LLM, embedding üzerinden ilgili veriyi çekerek soruya cevap üretir",
"LLM tek başına sayısal tahmin yapabilir ama kesinliği düşük",

"5. Önerilen Hibrit Mimari",
"User Question → StockAgent",
"Tahmin sorusu → ML.NET + CSV → Sayısal tahmin",
"Genel soru → Qdrant + LLM → Açıklama / Trend",

"6. Özet Tavsiyeler",
"Python bilmeden tamamen .NET ile agent geliştirebilirsin",
"LLM → Genel sorular ve yorumlar",
"ML.NET → Kesin sayısal tahmin",
"Vector DB → Semantic search ve RAG",
"Agent soruya göre doğru yöntemi seçer"
