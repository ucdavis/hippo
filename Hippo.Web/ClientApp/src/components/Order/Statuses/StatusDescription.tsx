import React from "react";
import { OrderStatus, orderStatusDescriptions } from "./status";
import { CardText } from "reactstrap";

interface StatusDescriptionProps {
  status: OrderStatus;
  children?: React.ReactNode;
}

const StatusDescription: React.FC<StatusDescriptionProps> = ({
  status,
  children,
}) => {
  const { description } = orderStatusDescriptions[status];

  return (
    <>
      <CardText>
        {description} {children}
      </CardText>
    </>
  );
};

export default StatusDescription;
