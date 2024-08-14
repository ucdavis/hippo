import React from "react";
import { OrderStatus, orderStatusDescriptions } from "../../types/status";
import { CardText } from "reactstrap";

interface StatusDescriptionProps {
  status: OrderStatus;
}

const StatusDescription: React.FC<StatusDescriptionProps> = ({ status }) => {
  return (
    <CardText>
      {orderStatusDescriptions[status].description}
      {orderStatusDescriptions[status].forAdmin}
      {orderStatusDescriptions[status].forSponsor}
    </CardText>
  );
};

export default StatusDescription;
