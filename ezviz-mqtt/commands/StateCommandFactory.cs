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
            var booleanConverter = new BooleanConvertor(jsonOptions.Value);
            StatePublishCommands = new Dictionary<string, IStatePublishCommand>(StringComparer.OrdinalIgnoreCase)
            {
                { StateEntities.AlarmDetectionMethod, new AlarmDetectionMethodCommand(topics,booleanConverter, mqttHandler) },
                { StateEntities.AlarmScheduleEnabled, new AlarmScheduleEnabledCommand(topics,booleanConverter, mqttHandler) },
                { StateEntities.AlarmSound, new AlarmSoundCommand(topics,booleanConverter, mqttHandler) },
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
                { StateEntities.TriggerAlarm, new TriggerAlarmStatusCommand(topics,booleanConverter, mqttHandler) },
                { StateEntities.UpgradeAvailable, new UpgradeAvailableCommand(topics,booleanConverter, mqttHandler) },
                { StateEntities.UpgradeInProgress, new UpgradeInProgressCommand(topics,booleanConverter, mqttHandler) },
                { StateEntities.UpgradePercent, new UpgradePercentCommand(topics,booleanConverter, mqttHandler) },
            };

            StateUpdateCommands = new Dictionary<string, IStateUpdateCommand>(StringComparer.OrdinalIgnoreCase)
            {
                { StateEntities.AlarmDetectionMethod, new AlarmDetectionMethodUpdateCommand(client, jsonOptions.Value, StatePublishCommands[StateEntities.AlarmDetectionMethod]) },
                { StateEntities.AlarmSound, new AlarmSoundUpdateCommand(client, jsonOptions.Value, StatePublishCommands[StateEntities.AlarmSound]) },
                { StateEntities.Armed, new ArmDisarmCommand(client, jsonOptions.Value, StatePublishCommands[StateEntities.Armed]) },
                { StateEntities.AudioEnabled, new AudioEnabledUpdateCommand(client, jsonOptions.Value, StatePublishCommands[StateEntities.AudioEnabled]) },
                { StateEntities.DetectionSensitivityLevel, new DetectionSensitivityUpdateCommand(client, jsonOptions.Value, StatePublishCommands[StateEntities.DetectionSensitivityLevel]) },
                { StateEntities.InfraredEnabled, new InfraredEnabledUpdateCommand(client, jsonOptions.Value, StatePublishCommands[StateEntities.InfraredEnabled]) },
                { StateEntities.MobileTrackingEnabled, new MobileTrackingEnabledUpdateCommand(client, jsonOptions.Value, StatePublishCommands[StateEntities.MobileTrackingEnabled]) },
                { StateEntities.Sleeping, new SleepingUpdateCommand(client, jsonOptions.Value, StatePublishCommands[StateEntities.Sleeping]) },
                { StateEntities.StatusLedEnabled, new StatusLedEnabledUpdateCommand(client, jsonOptions.Value, StatePublishCommands[StateEntities.StatusLedEnabled]) },
                { StateEntities.TriggerAlarm, new TriggerAlarmCommand(client, jsonOptions.Value, StatePublishCommands[StateEntities.TriggerAlarm]) },
                
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
