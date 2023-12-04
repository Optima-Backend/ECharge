using System;
namespace ECharge.Domain.Enums
{
    /// <summary>
    /// Represents various charging session statuses.
    /// </summary>
    public enum FinishReason
    {
        /// <summary>
        /// Charging was stopped by removing the cable in plug and charge mode.
        /// </summary>
        PLUG_AND_CHARGE_STOP,

        /// <summary>
        /// Alarm triggered during the charging session.
        /// </summary>
        CHARGER_ALARM,

        /// <summary>
        /// Couldn't start the charging session.
        /// </summary>
        CHARGING_START_FAIL,

        /// <summary>
        /// The charging session was stopped by the owner of the charger.
        /// </summary>
        REQUESTED_BY_OWNER,

        /// <summary>
        /// The charging session was finished because an EV didn't charge for more than 10 minutes.
        /// </summary>
        CHARGING_LOW_POWER,

        /// <summary>
        /// The charging session was finished because a cable was removed for more than 30 seconds.
        /// </summary>
        REQUESTED_BY_CABLE_STATE,

        /// <summary>
        /// 
        /// </summary>
        REQUESTED_BY_CLIENT,

        /// <summary>
        /// 
        /// </summary>
        REQUESTED_BY_CPO,


    }

}

