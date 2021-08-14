import { Box, styled, } from '@material-ui/core';

const CenteredBox = styled(Box)(
  {
    root: {
      display: 'flex',
      justifyContent: 'space-between',
      alignItems: 'center',
      textAlign: 'center',
    },
  }
);

export default CenteredBox;
