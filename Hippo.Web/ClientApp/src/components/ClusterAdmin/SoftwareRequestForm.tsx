import { useContext, useState } from "react";
import "react-bootstrap-typeahead/css/Typeahead.css";
import AppContext from "../../Shared/AppContext";
import { ClusterName, SoftwareRequestModel } from "../../types";
import { ClusterNames } from "../../constants";
import { authenticatedFetch, parseBadRequest } from "../../util/api";
import { usePromiseNotification } from "../../util/Notifications";
import { SearchPerson } from "../../Shared/SearchPerson";
import HipMainWrapper from "../../Shared/Layout/HipMainWrapper";
import HipTitle from "../../Shared/Layout/HipTitle";
import HipBody from "../../Shared/Layout/HipBody";
import HipButton from "../../Shared/HipButton";

const defaultSoftwareRequestModel = {
  clusterName: "",
  email: "",
  accountName: "",
  accountKerberos: "",
  softwareTitle: "",
  softwareLicense: "",
  softwareHomePage: "",
  benefitDescription: "",
  additionalInformation: "",
} as unknown as SoftwareRequestModel;

export const SoftwareRequestForm = () => {
  const [context] = useContext(AppContext);
  const [notification, setNotification] = usePromiseNotification();
  const [softwareRequestModel, setSoftwareRequestModel] =
    useState<SoftwareRequestModel>({
      ...defaultSoftwareRequestModel,
      email: context.user.detail.email,
      accountName: context.user.detail.name,
      accountKerberos: context.user.detail.kerberos,
    });

  const [user, setUser] = useState({ ...context.user.detail });

  const handleSubmit = async () => {
    const req = authenticatedFetch(`/api/software/requestinstall`, {
      method: "POST",
      body: JSON.stringify(softwareRequestModel),
    });

    setNotification(req, "Sending", "Request sent.", async (r) => {
      if (r.status === 400) {
        const errors = await parseBadRequest(response);
        return errors;
      } else {
        return "An error happened, please try again.";
      }
    });

    const response = await req;

    if (response.ok) {
      setSoftwareRequestModel({
        ...defaultSoftwareRequestModel,
        email: context.user.detail.email,
        accountName: context.user.detail.name,
        accountKerberos: context.user.detail.kerberos,
      });
    }
  };

  return (
    <HipMainWrapper>
      <HipTitle
        title={`Welcome, ${context.user.detail.firstName}`}
        subtitle="Software Request"
      />
      <HipBody>
        <p>
          Please enter the following details to request a custom installation of
          software...
        </p>
        <hr />
        <div className="form-group">
          <label>Cluster Name</label>
          <select
            className="form-control"
            value={softwareRequestModel.clusterName}
            onChange={(e) =>
              setSoftwareRequestModel({
                ...softwareRequestModel,
                clusterName: e.target.value as ClusterName,
              })
            }
          >
            <option value="">Select a cluster...</option>
            {ClusterNames.map((cluster) => (
              <option key={cluster} value={cluster}>
                {cluster}
              </option>
            ))}
          </select>
        </div>
        <div className="form-group">
          <label>UC Davis or Affiliate Email</label>
          <input
            type="email"
            className="form-control"
            value={softwareRequestModel.email}
            onChange={(e) =>
              setSoftwareRequestModel({
                ...softwareRequestModel,
                email: e.target.value,
              })
            }
            placeholder="Enter an email address"
          />
        </div>
        <div className="form-group">
          <label>Account Name</label>
          <SearchPerson
            user={user}
            onChange={(user) => {
              setSoftwareRequestModel({
                ...softwareRequestModel,
                accountName: user?.name,
                accountKerberos: user?.kerberos,
              });
              setUser(user);
            }}
          />
        </div>
        <div className="form-group">
          <label>Software Title</label>
          <input
            type="text"
            className="form-control"
            value={softwareRequestModel.softwareTitle}
            onChange={(e) =>
              setSoftwareRequestModel({
                ...softwareRequestModel,
                softwareTitle: e.target.value,
              })
            }
            placeholder="Enter the software title"
          />
        </div>
        <div className="form-group">
          <label>Software License</label>
          <textarea
            className="form-control"
            value={softwareRequestModel.softwareLicense}
            onChange={(e) =>
              setSoftwareRequestModel({
                ...softwareRequestModel,
                softwareLicense: e.target.value,
              })
            }
            placeholder="Enter the software license"
          />
        </div>
        <div className="form-group">
          <label>Software Home Page</label>
          <input
            type="text"
            className="form-control"
            value={softwareRequestModel.softwareHomePage}
            onChange={(e) =>
              setSoftwareRequestModel({
                ...softwareRequestModel,
                softwareHomePage: e.target.value,
              })
            }
            placeholder="Enter the software home page"
          />
        </div>
        <div className="form-group">
          <label>Benefit Description</label>
          <textarea
            className="form-control"
            value={softwareRequestModel.benefitDescription}
            onChange={(e) =>
              setSoftwareRequestModel({
                ...softwareRequestModel,
                benefitDescription: e.target.value,
              })
            }
            placeholder="Enter the benefit description"
          />
        </div>
        <div className="form-group">
          <label>Additional Information</label>
          <textarea
            className="form-control"
            value={softwareRequestModel.additionalInformation}
            onChange={(e) =>
              setSoftwareRequestModel({
                ...softwareRequestModel,
                additionalInformation: e.target.value,
              })
            }
            placeholder="Enter any additional information"
          />
        </div>
        <br />
        <HipButton disabled={notification.pending} onClick={handleSubmit}>
          Submit
        </HipButton>
        <div>
          <p className="form-helper"></p>
        </div>
      </HipBody>
    </HipMainWrapper>
  );
};
