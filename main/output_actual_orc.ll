; ModuleID = 'repl_module'
source_filename = "repl_module"

@x = external global double

define double @__anon_expr_5() {
entry:
  store double 4.000000e+00, ptr @x, align 8
  %x = load double, ptr @x, align 8
  %fmultmp = fmul double %x, 2.000000e+00
  ret double %fmultmp
}
