import React from "react";
import { OrderStatus, orderStatusDescriptions } from "../../types/status";
import { CardText } from "reactstrap";

interface StatusDescriptionProps {
  status: OrderStatus;
  isAdmin?: boolean;
}

const StatusDescription: React.FC<StatusDescriptionProps> = ({
  status,
  isAdmin,
}) => {
  return (
    <CardText>
      <p>{orderStatusDescriptions[status].description} </p>
      {orderStatusDescriptions[status].forAdmin}
      {orderStatusDescriptions[status].forSponsor}
    </CardText>
  );
};

export default StatusDescription;
