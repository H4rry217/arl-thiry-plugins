using AimRobot.Api;
using AimRobot.Api.config;
using AimRobot.Api.plugin;

namespace bfvrobot_anticheat {
    public class BfvRobotPlugin : PluginBase {

        public static AutoSaveConfig config;
        public static BfvRobotPlugin plugin;

        private IAntiCheat antiCheat;

        public override string GetAuthor() {
            return "H4rry217";
        }

        public override string GetDescription() {
            return "屏蔽BFV ROBOT社区全局黑名单玩家的插件";
        }

        public override string GetPluginName() {
            return "bfvrobot_anticheat";
        }

        public override Version GetVersion() {
            return new Version(1, 0, 0);
        }

        public override void OnDisable() {
            Robot.GetInstance().GetGameContext().UnregisterAntiCheat(this.antiCheat);
        }

        public override void OnEnable() {
            Robot.GetInstance().GetGameContext().RegisterAntiCheat(this.antiCheat);
        }

        public override void OnLoad() {
            plugin = this;
            config = Robot.GetInstance().GetPluginManager().GetDefaultAutoSaveConfig(this, "bfvrobot");
            Robot.GetInstance().GetPluginManager().ConfigAutoSave(config);

            this.antiCheat = new BfvRobotAntiCheat();

        }

    }
}