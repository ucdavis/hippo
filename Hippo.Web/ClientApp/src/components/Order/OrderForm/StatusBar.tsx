import React from "react";
import { Progress } from "reactstrap";
import { OrderStatus, compareOrderStatus } from "../../../types/status";
import HipProgress from "../../../Shared/HipProgress";

interface StatusBarProps {
  max?: number;
  status: OrderStatus;
  animated?: boolean;
  showInProgress?: boolean;
  showOnHover?: OrderStatus | null;
}

const StatusBar: React.FC<StatusBarProps> = ({
  max = 5,
  status,
  animated,
  showInProgress,
  showOnHover,
}) => {
  return (
    <div className="hip-progress status-bar">
      <h4>Order Status:</h4>
      <Progress multi>
        {(status === OrderStatus.Cancelled ||
          showOnHover === OrderStatus.Cancelled) && (
          <HipProgress
            label={OrderStatus.Cancelled}
            max={max}
            value={max / 2}
            color="danger"
            shouldFill={true}
            inProgress={showOnHover === OrderStatus.Cancelled}
            animated={false}
          />
        )}
        {(status === OrderStatus.Rejected ||
          showOnHover === OrderStatus.Rejected) && (
          <HipProgress
            label={OrderStatus.Rejected}
            max={max}
            value={max}
            color="danger"
            shouldFill={true}
            inProgress={showOnHover === OrderStatus.Rejected}
            animated={false}
          />
        )}
        {status !== OrderStatus.Cancelled &&
          showOnHover !== OrderStatus.Cancelled &&
          status !== OrderStatus.Rejected &&
          showOnHover !== OrderStatus.Rejected &&
          status !== OrderStatus.Archived &&
          showOnHover !== OrderStatus.Archived && (
            <>
              <HipProgress
                label={OrderStatus.Created}
                max={max}
                shouldFill={
                  compareOrderStatus(status, OrderStatus.Created) >= 0
                }
                inProgress={
                  showOnHover === OrderStatus.Created ||
                  (status === OrderStatus.Draft && showInProgress)
                }
                animated={status === OrderStatus.Draft && animated}
              />
              <HipProgress
                label={OrderStatus.Submitted}
                max={max}
                shouldFill={
                  compareOrderStatus(status, OrderStatus.Submitted) >= 0
                }
                inProgress={
                  showOnHover === OrderStatus.Submitted ||
                  (status === OrderStatus.Created && showInProgress)
                }
                animated={status === OrderStatus.Created && animated}
              />
              <HipProgress
                label={OrderStatus.Processing}
                max={max}
                shouldFill={
                  compareOrderStatus(status, OrderStatus.Processing) >= 0
                }
                inProgress={
                  showOnHover === OrderStatus.Processing ||
                  (status === OrderStatus.Submitted && showInProgress)
                }
                animated={status === OrderStatus.Submitted && animated}
              />
              <HipProgress
                label={OrderStatus.Active}
                max={max}
                shouldFill={compareOrderStatus(status, OrderStatus.Active) >= 0}
                inProgress={
                  showOnHover === OrderStatus.Active ||
                  (status === OrderStatus.Processing && showInProgress)
                }
                animated={status === OrderStatus.Processing && animated}
              />
              <HipProgress
                label={OrderStatus.Completed}
                max={max}
                shouldFill={
                  compareOrderStatus(status, OrderStatus.Completed) >= 0
                }
                inProgress={
                  showOnHover === OrderStatus.Completed ||
                  (status === OrderStatus.Active && showInProgress)
                }
                animated={status === OrderStatus.Active && animated}
              />
            </>
          )}
        {(status === OrderStatus.Archived ||
          showOnHover === OrderStatus.Archived) && (
          <HipProgress
            label={OrderStatus.Archived}
            max={max}
            value={max}
            color="secondary"
            shouldFill={true}
            inProgress={showOnHover === OrderStatus.Archived}
            striped={false}
          />
        )}
      </Progress>
    </div>
  );
};

export default StatusBar;
