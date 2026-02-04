using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

// ==========================================
// [SECTION 1] 실행 로직
// ==========================================

// 1. 설정 파일 읽기
if (!File.Exists("config.json"))
{
    Console.WriteLine("config.json 파일이 없습니다! 실행을 중단합니다.");
    return;
}

string jsonString = File.ReadAllText("config.json");
var config = JsonSerializer.Deserialize<AppConfig>(jsonString);

// [안전 장치] config 파일 내용이 비어있는지 체크
if (config is null)
{
    Console.WriteLine("config.json 형식이 잘못되었습니다.");
    return;
}

// 2. 변수에 할당
string riotApiKey = config.RiotApiKey;
string discordWebhookUrl = config.DiscordWebhookUrl;
string gameName = config.GameName;
string tagLine = config.TagLine;

Console.WriteLine("롤 정보 조회 시작...");

// 3. HTTP 클라이언트 생성 및 헤더 설정
using var client = new HttpClient();

// 🔥 [중요] 아까 빠졌던 부분입니다! (이게 없어서 401 에러가 났음)
client.DefaultRequestHeaders.Add("X-Riot-Token", riotApiKey); 

try
{
    // [단계 1] Riot ID로 PUUID 구하기 (asia 서버)
    string accountUrl = $"https://asia.api.riotgames.com/riot/account/v1/accounts/by-riot-id/{gameName}/{tagLine}";
    var accountInfo = await client.GetFromJsonAsync<AccountDto>(accountUrl);

    if (accountInfo is null) throw new Exception("계정 정보를 찾을 수 없습니다.");
    string puuid = accountInfo.puuid;

    Console.WriteLine($"[1단계 성공] PUUID 획득 완료");

    // [단계 2] PUUID로 소환사 정보 구하기 (kr 서버)
    string summonerUrl = $"https://kr.api.riotgames.com/lol/summoner/v4/summoners/by-puuid/{puuid}";
    var summoner = await client.GetFromJsonAsync<SummonerDto>(summonerUrl);

    if (summoner is null) throw new Exception("소환사 정보를 찾을 수 없습니다.");

    Console.WriteLine($"[2단계 성공] {summoner.name} (Lv.{summoner.summonerLevel})");

    // [단계 3] 디스코드 전송
    var msg = new
    {
        content = $"🔍 **{gameName}#{tagLine}** 검색 결과\n" +
                  $"- 레벨: {summoner.summonerLevel}\n" +
                  $"- PUUID: {puuid.Substring(0, 10)}..."
    };

    var jsonContent = new StringContent(JsonSerializer.Serialize(msg), Encoding.UTF8, "application/json");
    var response = await client.PostAsync(discordWebhookUrl, jsonContent);

    if (response.IsSuccessStatusCode)
        Console.WriteLine("✅ 디스코드 전송 완료!");
    else
        Console.WriteLine($"❌ 디스코드 실패: {response.StatusCode}");

}
catch (HttpRequestException httpEx)
{
    if (httpEx.StatusCode == System.Net.HttpStatusCode.Unauthorized) // 401 에러
    {
        Console.WriteLine("\n[인증 실패] 401 Unauthorized");
        Console.WriteLine("-> 키가 헤더에 제대로 안 들어갔거나, 키가 만료되었습니다.");
    }
    else if (httpEx.StatusCode == System.Net.HttpStatusCode.Forbidden) // 403 에러
    {
        Console.WriteLine("\n[금지됨] 403 Forbidden");
        Console.WriteLine("-> API Key 만료됨. 재발급 필요!");
    }
    else
    {
        Console.WriteLine($"\n[HTTP 에러] {httpEx.StatusCode}");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"\n[에러 발생] {ex.Message}");
}

// ==========================================
// [SECTION 2] 클래스/레코드 정의
// ==========================================

// 경고 해결: string 뒤에 '?'를 붙여서 "null일 수도 있다"고 알려주거나, 기본값을 줍니다.
public class AppConfig
{
    public string RiotApiKey { get; set; } = string.Empty;
    public string DiscordWebhookUrl { get; set; } = string.Empty;
    public string GameName { get; set; } = string.Empty;
    public string TagLine { get; set; } = string.Empty;
}

public record AccountDto(string puuid, string gameName, string tagLine);
public record SummonerDto(string id, string accountId, string puuid, string name, long summonerLevel);