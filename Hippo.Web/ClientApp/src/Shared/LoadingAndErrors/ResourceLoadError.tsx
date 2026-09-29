import NotFound from "../../NotFound";
import HipBody from "../Layout/HipBody";
import HipMainWrapper from "../Layout/HipMainWrapper";
import HipClientError from "./HipClientError";
import NotAuthorized from "./NotAuthorized";

export interface LoadError {
  // Network and JSON parsing failures have no HTTP status.
  status?: number;
}

export const ResourceLoadError = ({
  error,
  resource,
}: {
  error: LoadError;
  resource: string;
}) => {
  if (error.status === 404) return <NotFound />;
  if (error.status === 401 || error.status === 403) return <NotAuthorized />;
  return (
    <HipMainWrapper>
      <HipBody>
        <HipClientError type="alert" thereWasAnErrorLoadingThe={resource} />
      </HipBody>
    </HipMainWrapper>
  );
};
