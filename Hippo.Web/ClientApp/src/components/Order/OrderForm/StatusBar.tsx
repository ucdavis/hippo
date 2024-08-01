import React from "react";
import { Progress } from "reactstrap";
import { OrderStatus, compareOrderStatus } from "../../../types/status";
import HipProgress from "../../../Shared/HipProgress";

interface StatusBarProps {
  status: OrderStatus;
  animated?: boolean;
  showInProgress?: boolean;
}

const StatusBar: React.FC<StatusBarProps> = ({
  status,
  animated,
  showInProgress,
}) => {
  const max = 5;
  // TODO: handle cancelled and rejected statuses

  return (
    <div className="hip-progress">
      <Progress multi>
        {status === OrderStatus.Cancelled && (
          <HipProgress
            label={OrderStatus.Cancelled}
            max={max}
            value={max / 2}
            color="danger"
            shouldFill={true}
            inProgress={false}
            animated={false}
          />
        )}
        {status === OrderStatus.Rejected && (
          <HipProgress
            label={OrderStatus.Rejected}
            max={max}
            value={max}
            color="danger"
            shouldFill={true}
            inProgress={false}
            animated={false}
          />
        )}
        {status !== OrderStatus.Cancelled &&
          status !== OrderStatus.Rejected && (
            <>
              <HipProgress
                label={OrderStatus.Created}
                max={max}
                shouldFill={
                  compareOrderStatus(status, OrderStatus.Created) >= 0
                }
                inProgress={status === OrderStatus.Draft && showInProgress}
                animated={status === OrderStatus.Draft && animated}
              />
              <HipProgress
                label={OrderStatus.Submitted}
                max={max}
                shouldFill={
                  compareOrderStatus(status, OrderStatus.Submitted) >= 0
                }
                inProgress={status === OrderStatus.Created}
                animated={status === OrderStatus.Created && animated}
              />
              <HipProgress
                label={OrderStatus.Processing}
                max={max}
                shouldFill={
                  compareOrderStatus(status, OrderStatus.Processing) >= 0
                }
                inProgress={status === OrderStatus.Submitted}
                animated={status === OrderStatus.Submitted && animated}
              />
              <HipProgress
                label={OrderStatus.Active}
                max={max}
                shouldFill={compareOrderStatus(status, OrderStatus.Active) >= 0}
                inProgress={status === OrderStatus.Processing}
                animated={status === OrderStatus.Processing && animated}
              />
              <HipProgress
                label={OrderStatus.Completed}
                max={max}
                shouldFill={
                  compareOrderStatus(status, OrderStatus.Completed) >= 0
                }
                inProgress={status === OrderStatus.Active}
                animated={status === OrderStatus.Active && animated}
              />
            </>
          )}
      </Progress>
      <hr />
    </div>
  );
};

export default StatusBar;
