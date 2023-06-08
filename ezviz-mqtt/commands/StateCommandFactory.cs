using ezviz.net;
using ezviz_mqtt.commands.publish_state;
using ezviz_mqtt.commands.state_updates;
using ezviz_mqtt.config;
using ezviz_mqtt.util;
using Microsoft.Extensions.Options;

namespace ezviz_mqtt.commands
{

    internal class StateCommandFactory : IStateCommandFactory
    {
        private readonly Dictionary<string, IStateUpdateCommand> StateUpdateCommands;
        private readonly Dictionary<string, IStatePublishCommand> StatePublishCommands;

        public StateCommandFactory(IEzvizClient client, TopicExtensions topics, IMqttHandler mqttHandler, IOptions<JsonOptions> jsonOptions)
        {
            StateUpdateCommands = new Dictionary<string, IStateUpdateCommand>
            {
                //{ StateEntities.AlarmDetectionMethod, new AlarmDetectionMethodCommand(client, topics, publisher) },
                //{ StateEntities.AlarmScheduleEnabled, new ArmDisarmCommand(client, topics, publisher) },
                //{ StateEntities.AlarmSound, new ArmDisarmCommand(client, topics, publisher) },
                { StateEntities.Armed, new ArmDisarmCommand(client, topics, mqttHandler) },
                //{ StateEntities.AudioEnabled, new ArmDisarmCommand(client, topics, publisher) },
                //{ StateEntities.BatteryLevel, new ArmDisarmCommand(client, topics, publisher) },
                //{ StateEntities.DetectionSensitivityLevel, new ArmDisarmCommand(client, topics, publisher) },
                //{ StateEntities.DiskCapacity, new ArmDisarmCommand(client, topics, publisher) },
                //{ StateEntities.InfraredEnabled, new ArmDisarmCommand(client, topics, publisher) },
                //{ StateEntities.LastAlarm, new ArmDisarmCommand(client, topics, publisher) },
                //{ StateEntities.MobileTrackingEnabled, new ArmDisarmCommand(client, topics, publisher) },
                //{ StateEntities.NotifyWhenOffline, new ArmDisarmCommand(client, topics, publisher) },
                //{ StateEntities.PirStatus, new ArmDisarmCommand(client, topics, publisher) },
                //{ StateEntities.RtspEncrypted, new ArmDisarmCommand(client, topics, publisher) },
                //{ StateEntities.Sleeping, new ArmDisarmCommand(client, topics, publisher) },
                //{ StateEntities.StatusLedEnabled, new ArmDisarmCommand(client, topics, publisher) },
                //{ StateEntities.UpgradeAvailable, new ArmDisarmCommand(client, topics, publisher) },
                //{ StateEntities.UpgradeInProgress, new ArmDisarmCommand(client, topics, publisher) },
                //{ StateEntities.UpgradePercent, new ArmDisarmCommand(client, topics, publisher) },

            };

            var booleanConverter = new BooleanConvertor(jsonOptions.Value);
            StatePublishCommands = new Dictionary<string, IStatePublishCommand>
            {
                { StateEntities.AlarmDetectionMethod, new AlarmDetectionMethodCommand(topics,booleanConverter, mqttHandler) },
                { StateEntities.AlarmScheduleEnabled, new AlarmScheduleEnabledCommand(topics,booleanConverter, mqttHandler) },
                { StateEntities.AlarmSound, new ArmedCommand(topics,booleanConverter, mqttHandler) },
                { StateEntities.Armed, new ArmedCommand(topics,booleanConverter, mqttHandler) },
                { StateEntities.AudioEnabled, new AudioEnabledCommand(topics,booleanConverter, mqttHandler) },
                { StateEntities.BatteryLevel, new BatteryLevelCommand(topics,booleanConverter, mqttHandler) },
                { StateEntities.DetectionSensitivityLevel, new DetectionSensitivityCommand(topics,booleanConverter, mqttHandler) },
                { StateEntities.DiskCapacity, new DiskCapacityCommand(topics,booleanConverter, mqttHandler) },
                { StateEntities.InfraredEnabled, new InfraredEnabledCommand(topics,booleanConverter, mqttHandler) },
                { StateEntities.LastAlarm, new LastAlarmCommand(topics,booleanConverter, mqttHandler) },
                { StateEntities.MobileTrackingEnabled, new MobileTrackingEnabledCommand(topics,booleanConverter, mqttHandler) },
                { StateEntities.NotifyWhenOffline, new NotifyWhenOfflineCommand(topics,booleanConverter, mqttHandler) },
                { StateEntities.PirStatus, new PirStatusCommand(topics,booleanConverter, mqttHandler) },
                { StateEntities.RtspEncrypted, new RtspEncryptedCommand(topics,booleanConverter, mqttHandler) },
                { StateEntities.Sleeping, new SleepingCommand(topics,booleanConverter, mqttHandler) },
                { StateEntities.StatusLedEnabled, new StatusLedEnabledCommand(topics,booleanConverter, mqttHandler) },
                { StateEntities.UpgradeAvailable, new UpgradeAvailableCommand(topics,booleanConverter, mqttHandler) },
                { StateEntities.UpgradeInProgress, new UpgradeInProgressCommand(topics,booleanConverter, mqttHandler) },
                { StateEntities.UpgradePercent, new UpgradePercentCommand(topics,booleanConverter, mqttHandler) },
            };
        }

        public IStateUpdateCommand GetStateUpdateCommand(string commandKey)
        {
            var command = StateUpdateCommands.ContainsKey(commandKey) ? StateUpdateCommands[commandKey] : null;
            if (command == null)
            {
                throw new Exception($"Unknown state update command [{commandKey}]");
            }
            return command;
        }

        public IStatePublishCommand GetStatePublishCommand(string commandKey)
        {
            var command = StatePublishCommands.ContainsKey(commandKey) ? StatePublishCommands[commandKey] : null;
            if (command == null)
            {
                throw new Exception($"Unknown state publish command [{commandKey}]");
            }
            return command;
        }

        public IEnumerable<IStatePublishCommand> GetAllStatePublishCommands()
        {
            return StatePublishCommands.Values;
        }
    }
}
