# 🌟 Caching Setup

---

## 1️⃣ **캐싱하고 싶은 클래스에 `CacheUtil`을 상속받는다.**  
클래스를 작성할 때, `CacheUtil`을 상속받으면 캐싱 기능을 사용할 수 있습니다.  
이렇게 하여 캐싱 로직을 추가해 주세요.

---

## 2️⃣ **Caching → Generate Script 순으로 인스펙터 창에서 실행한다.**  
인스펙터 창에서 **Caching**을 먼저 클릭하고, 그 후 **Generate Script**를 실행합니다.  
⚠️ 이 순서를 반드시 지켜야 합니다!

---

## 3️⃣ **코드에서 `RJ_TC` 네임스페이스 안에 있는 `C`에 접근하여 `C.G_TC(특정 class)`로 접근한다.**  
`RJ_TC.C.G_TC`를 사용하여 원하는 클래스를 호출할 수 있습니다.  
이 방법으로 필요한 클래스를 쉽게 접근할 수 있습니다.

---

# 🌟 Caching Setup (English)

---

## 1️⃣ **Inherit `CacheUtil` in the class you want to cache.**  
By inheriting `CacheUtil`, you can implement caching in your class.

---

## 2️⃣ **Run in Inspector in the order of Caching → Generate Script.**  
Make sure to click **Caching** first, followed by **Generate Script**.  
⚠️ Always follow this sequence!

---

## 3️⃣ **Access `C` within the `RJ_TC` namespace and use `C.G_TC(specific class)` to access it.**  
You can easily access your specific class using `RJ_TC.C.G_TC`.

---

