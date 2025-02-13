# EditorTool
🚀 Caching Transformer for MonoBehaviour
📑 Overview
To efficiently cache transformations, inherit the CacheUtil class in the root object class.

⚙️ How it works:
Inherit CacheUtil
Inherit CacheUtil in the root class of your object to enable transformation caching.

Generate Script in Inspector
In the Unity Inspector, go to Caching => Generate to generate the caching script.

Access Variables
Use CM.G_TC(this).variable to access variables through the current instance (this => MonoBehaviour).







🚀 MonoBehaviour의 변환 캐싱
📑 개요
효율적인 변환 캐싱을 위해, 오브젝트의 루트 클래스에서 CacheUtil을 상속합니다.

⚙️ 작동 방식:
CacheUtil 상속
변환 캐싱을 활성화하려면 오브젝트 루트 클래스에서 CacheUtil을 상속합니다.

인스펙터에서 스크립트 생성
Unity 인스펙터에서 Caching => Generate를 통해 캐싱 스크립트를 생성합니다.

변수 접근
CM.G_TC(this).변수를 사용하여 변수에 접근합니다. (this => MonoBehaviour)
