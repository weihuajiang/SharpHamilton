using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Huarui.STARLine
{
    /// <summary>
    /// Main Error Enumeration
    /// </summary>
    public enum MainErrorEnum
    {
        /// <summary>
        /// No Error
        /// </summary>
        NoError = 0,

        /// <summary>
        /// There is a wrong set of parameters or parameter ranges.
        /// </summary>
        SyntaxError = 1,

        /// <summary>
        /// Steps lost on one or more hardware components, or component not initialized or not functioning.
        /// </summary>
        /// 
        HardwareError = 2,
        /// <summary>
        /// There was an error in previous part command.
        /// </summary>
        NotExecutedError = 3,

        /// <summary>
        /// Blood clot detected.
        /// </summary>
        ClotError = 4,

        /// <summary>
        /// Barcode could not be read or is missing.
        /// </summary>
        BarcodeError = 5,

        /// <summary>
        /// Not enough liquid available.
        /// </summary>
        InsufficientLiquidError = 6,

        /// <summary>
        /// A tip has already been picked up.
        /// </summary>
        TipPresentError = 7,

        /// <summary>
        /// Tip is missing or not picked up.
        /// </summary>
        NoTipError = 8,

        /// <summary>
        /// No carrier present for loading.
        /// </summary>
        NoCarrierError = 9,

        /// <summary>
        /// A step or a part of a step could not be processed.
        /// </summary>
        ExecutionError = 10,

        /// <summary>
        /// A dispense with pressure liquid level detection is not allowed.
        /// </summary>
        PressureLLDError = 11,

        /// <summary>
        /// No capacitive signal detected during carrier calibration procedure.
        /// </summary>
        CalibrateError = 12,

        /// <summary>
        /// Not possible to unload the carrier due to occupied loading tray position.
        /// </summary>
        UnloadError = 13,

        /// <summary>
        /// Pressure liquid level detection in a consecutive aspiration is not allowed.
        /// </summary>
        PressureLLDconsecutiveError = 14,

        /// <summary>
        /// Dispense in jet mode with pressure liquid level detection is not allowed.
        /// </summary>
        ParameterError = 15,

        /// <summary>
        /// Cover not closed or can not be locked.
        /// </summary>
        CoverOpenError = 16,

        /// <summary>
        /// The pressure-based aspiration / dispensation control reported an error ( not enough liquid ).
        /// </summary>
        ImproperAspirationDispenseError = 17,

        /// <summary>
        /// Waste full or no more wash liquid available.
        /// </summary>
        WashLiquidError = 18,

        /// <summary>
        /// Incubator temperature out of range.
        /// </summary>
        TemperatureError = 19,
        /// <summary>
        /// Overshot of limits during aspirate or dispense.
        /// Note:
        /// On aspirate this error is returned as main error 17.
        /// On dispense this error is returned as main error 4.
        /// </summary>
        /// 
        TADMOvershot = 20,

        /// <summary>
        /// Labware not available.
        /// </summary>
        LabwareError = 21,

        /// <summary>
        /// Labware already gripped.
        /// </summary>
        LabwareGrippedError = 22,

        /// <summary>
        /// Labware lost during transport.
        /// </summary>
        LabwareLostError = 23,

        /// <summary>
        /// Cannot place plate, plate was gripped in a wrong direction.
        /// </summary>
        IllegalTargetPlatePosition = 24,

        /// <summary>
        /// Cover was opened or a carrier was removed manually.
        /// </summary>
        IllegalInterventionError = 25,

        /// <summary>
        /// Undershot of limits during aspirate or dispense.
        /// Note:
        /// On aspirate this error is returned as main error 4.
        /// On dispense this error is returned as main error 17.
        /// </summary>
        TADMUndershot = 26,

        /// <summary>
        /// The position is out of range.
        /// </summary>
        PositionError = 27,

        /// <summary>
        /// The cLLD detected a liquid level above start height of liquid level search.
        /// </summary>
        UnexpectedcLLDError = 28,

        /// <summary>
        /// Instrument region already reserved.
        /// </summary>
        AreaAlreadyOccupied = 29,

        /// <summary>
        /// A region on the instrument cannot be reserved.
        /// </summary>
        ImpossibleToOccupyArea = 30,

        /// <summary>
        /// Anti drop controlling out of tolerance.
        /// </summary>
        AntiDropControlError = 31,

        /// <summary>
        /// Decapper lock error while screw / unscrew a cap by twister channels.
        /// </summary>
        DecapperError = 32,

        /// <summary>
        /// Decapper station error while lock / unlock a cap.
        /// </summary>
        DecapperHandlingError = 33,

        /// <summary>
        /// Slave error.
        /// </summary>
        SlaveError = 99,

        /// <summary>
        /// Wrong carrier barcode detected.
        /// </summary>
        WrongCarrierError = 100,

        /// <summary>
        /// Carrier barcode could not be read or is missing.
        /// </summary>
        NoCarrierBarcodeError = 101,

        /// <summary>
        /// Liquid surface not detected.This error is created from main / slave error 06/70, 06/73 and 06/87.
        /// </summary>
        LiquidLevelError = 102,

        /// <summary>
        /// Carrier not detected at deck end position.
        /// </summary>
        NotDetectedError = 103,

        /// <summary>
        /// Dispense volume exceeds the aspirated volume.
        /// This error is created from main / slave error 02/54.
        /// </summary>
        NotAspiratedError = 104,

        /// <summary>
        /// The dispensed volume is out of tolerance (may only occur for Nano Pipettor Dispense steps).
        /// This error is created from main / slave error 02/52 and 02/54.
        /// </summary>
        ImproperDispensationError = 105,

        /// <summary>
        /// The labware to be loaded was not detected by autoload module.
        /// Note:
        /// May only occur on a Reload Carrier step if the labware property 'MlStarCarPosAreRecognizable' is set to 1.
        /// </summary>
        NoLabwareError = 106,

        /// <summary>
        /// The labware contains unexpected barcode ( may only occur on a Reload Carrier step ).
        /// </summary>
        UnexpectedLabwareError = 107,

        /// <summary>
        /// The labware to be reloaded contains wrong barcode ( may only occur on a Reload Carrier step ).
        /// </summary>
        WrongLabwareError = 108,

        /// <summary>
        /// The barcode read doesn't match the barcode mask defined.
        /// </summary>
        BarcodeMaskError = 109,

        /// <summary>
        /// The barcode read is not unique. Previously loaded labware with same barcode was loaded without unique barcode check.
        /// </summary>
        BarcodeNotUniqueError = 110,

        /// <summary>
        /// The barcode read is already loaded as unique barcode ( it's not possible to load the same barcode twice ).
        /// </summary>
        BarcodeAlreadyUsedError = 111,

        /// <summary>
        /// Kit Lot expired.
        /// </summary>
        KitLotExpiredError = 112,

        /// <summary>
        /// Barcode contains character which is used as delimiter in result string.
        /// </summary>
        DelimiterError = 113

    }
}
