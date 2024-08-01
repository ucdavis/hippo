import React from "react";
import { Progress } from "reactstrap";
import { OrderStatus, compareOrderStatus } from "../../../types/status";
import HipProgress from "../../../Shared/HipProgress";

interface StatusBarProps {
  status: OrderStatus;
}

const StatusBar: React.FC<StatusBarProps> = ({ status }) => {
  const max = 5;
  // TODO: handle cancelled and rejected statuses

  return (
    <div className="hip-progress">
      <Progress multi>
        <HipProgress
          label={OrderStatus.Created}
          max={max}
          active={compareOrderStatus(status, OrderStatus.Created) >= 0}
          inProgress={status === OrderStatus.Draft}
        />
        <HipProgress
          label={OrderStatus.Submitted}
          max={max}
          active={compareOrderStatus(status, OrderStatus.Submitted) >= 0}
          inProgress={status === OrderStatus.Created}
        />
        <HipProgress
          label={OrderStatus.Processing}
          max={max}
          active={compareOrderStatus(status, OrderStatus.Processing) >= 0}
          inProgress={status === OrderStatus.Submitted}
        />
        <HipProgress
          label={OrderStatus.Active}
          max={max}
          active={compareOrderStatus(status, OrderStatus.Active) >= 0}
          inProgress={status === OrderStatus.Processing}
        />
        <HipProgress
          label={OrderStatus.Completed}
          max={max}
          active={compareOrderStatus(status, OrderStatus.Completed) >= 0}
          inProgress={status === OrderStatus.Active}
        />
      </Progress>
      <hr />
    </div>
  );
};

export default StatusBar;
