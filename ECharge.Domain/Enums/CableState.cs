namespace ECharge.Domain.Enums
{
    /// <summary>
    /// Represents the charging status of an electric vehicle.
    /// </summary>
    public enum CableState
    {
        /// <summary>
        /// No EV present: This status indicates that there is no electric vehicle (EV) present for charging at the charging station.
        /// </summary>
        A,

        /// <summary>
        /// EV present, no charging: This status indicates that an EV is present at the charging station, but it is not currently charging.
        /// </summary>
        B,

        /// <summary>
        /// Charging: This status indicates that the EV is actively charging at the charging station.
        /// </summary>
        C,

        /// <summary>
        /// Charging with Ventilation: This status indicates that the EV is charging, and there is also ventilation or cooling system in operation, likely to dissipate heat generated during charging.
        /// </summary>
        D,

        /// <summary>
        /// Error. No power on CP line: This status indicates an error where there is no power on the CP (Control Pilot) line, which is used for communication between the EV and the charging station. This can prevent charging from starting.
        /// </summary>
        E,

        /// <summary>
        /// Error: This status represents a generic error condition during the charging process that is not specifically related to any of the above scenarios.
        /// </summary>
        F
    }

}

