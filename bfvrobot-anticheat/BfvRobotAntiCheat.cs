using AimRobot.Api;
using AimRobot.Api.game;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Resources;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace bfvrobot_anticheat {
    public class BfvRobotAntiCheat : IAntiCheat {

        public delegate void DataCallback(object param);

        public void IsAbnormalPlayer(string name, IAntiCheat.CheckResult checkResult) {
            Robot.GetInstance().GetGameContext().GetPlayerStatInfo(name, (statData) => {
                IsAbnormalPlayer(statData.id, checkResult);
            });
        }

        public void IsAbnormalPlayer(long playerId, IAntiCheat.CheckResult checkResult) {
            var data = BfvRobotPlugin.config.GetData($"{playerId}");

            try {
                if (data != null) {
                    var vals = data.Split(",");

                    long curTime = DateTimeOffset.Now.ToUnixTimeSeconds();
                    long dataCreateTime = long.Parse(vals[1]);

                    if (((curTime - dataCreateTime) / (float)86400) < 3) {
                        int status = int.Parse(vals[0]);
                        if (GameConst.BfvRobotCommunityAbnormalStatus.ContainsKey(int.Parse(vals[0]))) {
                            checkResult(true, GameConst.BfvRobotCommunityAbnormalStatus[status], GameConst.BfvRobotCommunityAbnormalStatus[status]);
                        } else {
                            checkResult(false, null, null);
                        }
                        return;
                    }
                }
            }catch(Exception ex) {
                BfvRobotPlugin.plugin.GetLogger().Error($"从本地获取bfv robot数据发生异常！{ex}");
            }

            GetBfvRobotStat(playerId, (data) => {
                var json = ((JsonDocument)data).RootElement;
                string message = json.GetProperty("message").GetString();

                if (json.GetProperty("status").GetInt32() == 1) {
                    if (string.Equals(message, "successful")) {
                        try {
                            var status = (json.GetProperty("data")[0].GetString());
                            int statusInt = int.Parse(status);

                            if (GameConst.BfvRobotCommunityAbnormalStatus.ContainsKey(statusInt)) {
                                checkResult(true, GameConst.BfvRobotCommunityAbnormalStatus[statusInt], GameConst.BfvRobotCommunityAbnormalStatus[statusInt]);
                            } else {
                                checkResult(false, null, null);
                            }

                            BfvRobotPlugin.config.SetData($"{playerId}", $"{statusInt},{DateTimeOffset.Now.ToUnixTimeSeconds()}");

                        } catch (Exception ex) {
                            BfvRobotPlugin.plugin.GetLogger().Error($"获取bfv robot数据发生异常！{ex}");
                        }
                    } else {
                        checkResult(false, null, null);
                    }
                }
            });
        }

        public async void GetBfvRobotStat(long playerId, DataCallback callback) {
            string data = await Get($"https://api.zth.ink/api/findPlayerStatus?pid={playerId}");

            if (!string.Equals(string.Empty, data)) callback(JsonDocument.Parse(data));
        }

        public async Task<string> Get(string url) {

            using (HttpClient httpClient = new HttpClient()) {

                try {
                    HttpResponseMessage response = await httpClient.GetAsync(url);
                    if (response.IsSuccessStatusCode) {
                        return await response.Content.ReadAsStringAsync();
                    } else {
                        return string.Empty;
                    }
                } catch (HttpRequestException ex) {
                    return string.Empty;
                }
            }

        }

    }
}
