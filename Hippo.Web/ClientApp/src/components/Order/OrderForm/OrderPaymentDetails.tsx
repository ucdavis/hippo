import React from "react";
import { faDollarSign } from "@fortawesome/free-solid-svg-icons";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { convertToPacificDate } from "../../../util/DateHelper";
import HipFormField from "../../../Shared/Form/HipFormField";
import HipFormFieldReadOnly from "../../../Shared/Form/HipFormFieldReadOnly";

interface OrderPaymentDetailsProps {
  balancePending: string;
  balanceRemaining: string;
  nextPaymentDate: string;
  nextPaymentAmount: string;
}

const OrderPaymentDetails: React.FC<OrderPaymentDetailsProps> = ({
  balanceRemaining,
  balancePending,
  nextPaymentDate,
  nextPaymentAmount,
}) => {
  return (
    <>
      <h2>Payments</h2>
      <HipFormFieldReadOnly
        name="balanceRemaining"
        label="Balance Remaining"
        type="number"
        readOnly
        value={balanceRemaining}
        inputPrepend={<FontAwesomeIcon icon={faDollarSign} />}
      />
      {Number(balancePending) !== 0 && (
        <HipFormFieldReadOnly
          name="balancePending"
          label="Total Pending Payments"
          type="number"
          readOnly
          value={Number(balancePending).toFixed(2)}
          inputPrepend={<FontAwesomeIcon icon={faDollarSign} />}
        />
      )}
      {nextPaymentDate && (
        <>
          <HipFormFieldReadOnly
            name="nextPaymentDate"
            label="Next Payment Date"
            type="date"
            readOnly
            value={convertToPacificDate(nextPaymentDate)}
          />
          <HipFormFieldReadOnly
            name="nextPaymentAmount"
            label="Next Payment Amount"
            type="number"
            readOnly
            value={nextPaymentAmount}
            inputPrepend={<FontAwesomeIcon icon={faDollarSign} />}
          />
        </>
      )}
    </>
  );
};

export default OrderPaymentDetails;
