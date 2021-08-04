import { createStyles, Paper, Theme, withStyles } from '@material-ui/core';

const GlassPaper = withStyles((theme: Theme) =>
  createStyles({
    root: {
      backgroundColor:
        theme.palette.background.paper +
        (theme.palette.type == 'dark' ? '99' : '9'),
      padding: theme.spacing(4),
      boxShadow: theme.shadows[10],
      borderRadius: theme.shape.borderRadius * 3,
      backdropFilter: 'blur(5px)',

      [theme.breakpoints.down('sm')]: {
        width: '100%',
        height: '100%',
        margin: 0,
        borderRadius: 0,
        backdropFilter: 'none',
        backgroundColor: theme.palette.background.paper,
      },
    },
  }),
)(Paper);

export default GlassPaper;
