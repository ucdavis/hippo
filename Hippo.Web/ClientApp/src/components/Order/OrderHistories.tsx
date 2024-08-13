import HipBody from "../../Shared/Layout/HipBody";
import HipMainWrapper from "../../Shared/Layout/HipMainWrapper";
import HipTitle from "../../Shared/Layout/HipTitle";
import HipErrorBoundary from "../../Shared/LoadingAndErrors/HipErrorBoundary";
import HipClientError from "../../Shared/LoadingAndErrors/HipClientError";
import { HistoryTable } from "./HistoryTable";

export const OrderHistories: React.FC = () => {
  // RH TODO: get history info here to display name?
  return (
    <HipMainWrapper>
      <HipTitle title="" subtitle="Details" />
      <HipBody>
        <HipErrorBoundary
          fallback={
            <HipClientError
              type="alert"
              thereWasAnErrorLoadingThe="History Table"
              contactLink={true}
            />
          }
        >
          <HistoryTable
            numberOfRows={1000}
            showLinkToAll={false}
            historyCount={0}
          />
        </HipErrorBoundary>
      </HipBody>
    </HipMainWrapper>
  );
};
