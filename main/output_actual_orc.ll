; ModuleID = 'repl_module'
source_filename = "repl_module"

%dataframe = type { ptr, ptr, ptr }
%array = type { i64, i64, ptr }

@x = external global ptr
@__where_src = global ptr null, align 8
@str = private unnamed_addr constant [6 x i8] c"index\00", align 1
@str.1 = private unnamed_addr constant [5 x i8] c"name\00", align 1
@str.2 = private unnamed_addr constant [4 x i8] c"age\00", align 1
@str.3 = private unnamed_addr constant [7 x i8] c"hasJob\00", align 1
@str.4 = private unnamed_addr constant [8 x i8] c"savings\00", align 1
@__where_result = global ptr null, align 8
@__where_i = global i64 0, align 8
@str.5 = private unnamed_addr constant [6 x i8] c"index\00", align 1
@str.6 = private unnamed_addr constant [5 x i8] c"name\00", align 1
@str.7 = private unnamed_addr constant [4 x i8] c"age\00", align 1
@str.8 = private unnamed_addr constant [7 x i8] c"hasJob\00", align 1
@str.9 = private unnamed_addr constant [8 x i8] c"savings\00", align 1
@str.10 = private unnamed_addr constant [5 x i8] c"John\00", align 1

define ptr @main_2() {
entry:
  %x_load = load ptr, ptr @x, align 8
  store ptr %x_load, ptr @__where_src, align 8
  %arr_header = call ptr @malloc(i64 24)
  %arr_data = call ptr @malloc(i64 800000)
  %0 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header, i32 0, i32 0
  store i64 5, ptr %0, align 4
  %1 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header, i32 0, i32 1
  store i64 100000, ptr %1, align 4
  %2 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header, i32 0, i32 2
  store ptr %arr_data, ptr %2, align 8
  %elem_ptr = getelementptr ptr, ptr %arr_data, i64 0
  store ptr @str, ptr %elem_ptr, align 8
  %elem_ptr1 = getelementptr ptr, ptr %arr_data, i64 1
  store ptr @str.1, ptr %elem_ptr1, align 8
  %elem_ptr2 = getelementptr ptr, ptr %arr_data, i64 2
  store ptr @str.2, ptr %elem_ptr2, align 8
  %elem_ptr3 = getelementptr ptr, ptr %arr_data, i64 3
  store ptr @str.3, ptr %elem_ptr3, align 8
  %elem_ptr4 = getelementptr ptr, ptr %arr_data, i64 4
  store ptr @str.4, ptr %elem_ptr4, align 8
  %arr_header5 = call ptr @malloc(i64 24)
  %arr_data6 = call ptr @malloc(i64 800)
  %3 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header5, i32 0, i32 0
  store i64 0, ptr %3, align 4
  %4 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header5, i32 0, i32 1
  store i64 100, ptr %4, align 4
  %5 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header5, i32 0, i32 2
  store ptr %arr_data6, ptr %5, align 8
  %arr_header7 = call ptr @malloc(i64 24)
  %arr_data8 = call ptr @malloc(i64 800000)
  %6 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header7, i32 0, i32 0
  store i64 5, ptr %6, align 4
  %7 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header7, i32 0, i32 1
  store i64 100000, ptr %7, align 4
  %8 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header7, i32 0, i32 2
  store ptr %arr_data8, ptr %8, align 8
  %elem_ptr9 = getelementptr ptr, ptr %arr_data8, i64 0
  store ptr bitcast (i64 1 to ptr), ptr %elem_ptr9, align 8
  %elem_ptr10 = getelementptr ptr, ptr %arr_data8, i64 1
  store ptr bitcast (i64 4 to ptr), ptr %elem_ptr10, align 8
  %elem_ptr11 = getelementptr ptr, ptr %arr_data8, i64 2
  store ptr bitcast (i64 1 to ptr), ptr %elem_ptr11, align 8
  %elem_ptr12 = getelementptr ptr, ptr %arr_data8, i64 3
  store ptr bitcast (i64 3 to ptr), ptr %elem_ptr12, align 8
  %elem_ptr13 = getelementptr ptr, ptr %arr_data8, i64 4
  store ptr bitcast (i64 2 to ptr), ptr %elem_ptr13, align 8
  %df = tail call ptr @malloc(i32 ptrtoint (ptr getelementptr (%dataframe, ptr null, i32 1) to i32))
  %9 = getelementptr inbounds nuw %dataframe, ptr %df, i32 0, i32 0
  store ptr %arr_header, ptr %9, align 8
  %10 = getelementptr inbounds nuw %dataframe, ptr %df, i32 0, i32 1
  store ptr %arr_header5, ptr %10, align 8
  %11 = getelementptr inbounds nuw %dataframe, ptr %df, i32 0, i32 2
  store ptr %arr_header7, ptr %11, align 8
  store ptr %df, ptr @__where_result, align 8
  store i64 0, ptr @__where_i, align 8
  br label %for.cond

for.cond:                                         ; preds = %for.step, %entry
  %__where_i_load = load i64, ptr @__where_i, align 8
  %__where_src_load = load ptr, ptr @__where_src, align 8
  %rows_ptr_field = getelementptr inbounds nuw { ptr, ptr, ptr }, ptr %__where_src_load, i32 0, i32 1
  %rows_ptr = load ptr, ptr %rows_ptr_field, align 8
  %rows_array_ptr = bitcast ptr %rows_ptr to ptr
  %rows_length_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %rows_array_ptr, i32 0, i32 0
  %rows_length = load i64, ptr %rows_length_ptr, align 8
  %icmp_tmp = icmp slt i64 %__where_i_load, %rows_length
  br i1 %icmp_tmp, label %for.body, label %for.end

for.body:                                         ; preds = %for.cond
  %__where_src_load14 = load ptr, ptr @__where_src, align 8
  %__where_i_load15 = load i64, ptr @__where_i, align 8
  %rows_ptr_ptr = getelementptr inbounds nuw %dataframe, ptr %__where_src_load14, i32 0, i32 1
  %rows = load ptr, ptr %rows_ptr_ptr, align 8
  %data_ptr_ptr = getelementptr inbounds nuw %array, ptr %rows, i32 0, i32 2
  %data_ptr = load ptr, ptr %data_ptr_ptr, align 8
  %len_ptr = getelementptr inbounds nuw %array, ptr %rows, i32 0, i32 0
  %len = load i64, ptr %len_ptr, align 4
  %in_bounds = icmp ult i64 %__where_i_load15, %len
  br i1 %in_bounds, label %idx_ok, label %idx_err

for.step:                                         ; preds = %ifcont
  %inc_load = load i64, ptr @__where_i, align 4
  %inc_add = add i64 %inc_load, 1
  store i64 %inc_add, ptr @__where_i, align 8
  br label %for.cond

for.end:                                          ; preds = %for.cond
  %__where_result_load34 = load ptr, ptr @__where_result, align 8
  store ptr %__where_result_load34, ptr @__where_src, align 8
  %arr_header35 = call ptr @malloc(i64 24)
  %arr_data36 = call ptr @malloc(i64 800000)
  %12 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header35, i32 0, i32 0
  store i64 5, ptr %12, align 4
  %13 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header35, i32 0, i32 1
  store i64 100000, ptr %13, align 4
  %14 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header35, i32 0, i32 2
  store ptr %arr_data36, ptr %14, align 8
  %elem_ptr37 = getelementptr ptr, ptr %arr_data36, i64 0
  store ptr @str.5, ptr %elem_ptr37, align 8
  %elem_ptr38 = getelementptr ptr, ptr %arr_data36, i64 1
  store ptr @str.6, ptr %elem_ptr38, align 8
  %elem_ptr39 = getelementptr ptr, ptr %arr_data36, i64 2
  store ptr @str.7, ptr %elem_ptr39, align 8
  %elem_ptr40 = getelementptr ptr, ptr %arr_data36, i64 3
  store ptr @str.8, ptr %elem_ptr40, align 8
  %elem_ptr41 = getelementptr ptr, ptr %arr_data36, i64 4
  store ptr @str.9, ptr %elem_ptr41, align 8
  %arr_header42 = call ptr @malloc(i64 24)
  %arr_data43 = call ptr @malloc(i64 800)
  %15 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header42, i32 0, i32 0
  store i64 0, ptr %15, align 4
  %16 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header42, i32 0, i32 1
  store i64 100, ptr %16, align 4
  %17 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header42, i32 0, i32 2
  store ptr %arr_data43, ptr %17, align 8
  %arr_header44 = call ptr @malloc(i64 24)
  %arr_data45 = call ptr @malloc(i64 800000)
  %18 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header44, i32 0, i32 0
  store i64 5, ptr %18, align 4
  %19 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header44, i32 0, i32 1
  store i64 100000, ptr %19, align 4
  %20 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header44, i32 0, i32 2
  store ptr %arr_data45, ptr %20, align 8
  %elem_ptr46 = getelementptr ptr, ptr %arr_data45, i64 0
  store ptr bitcast (i64 1 to ptr), ptr %elem_ptr46, align 8
  %elem_ptr47 = getelementptr ptr, ptr %arr_data45, i64 1
  store ptr bitcast (i64 4 to ptr), ptr %elem_ptr47, align 8
  %elem_ptr48 = getelementptr ptr, ptr %arr_data45, i64 2
  store ptr bitcast (i64 1 to ptr), ptr %elem_ptr48, align 8
  %elem_ptr49 = getelementptr ptr, ptr %arr_data45, i64 3
  store ptr bitcast (i64 3 to ptr), ptr %elem_ptr49, align 8
  %elem_ptr50 = getelementptr ptr, ptr %arr_data45, i64 4
  store ptr bitcast (i64 2 to ptr), ptr %elem_ptr50, align 8
  %df51 = tail call ptr @malloc(i32 ptrtoint (ptr getelementptr (%dataframe, ptr null, i32 1) to i32))
  %21 = getelementptr inbounds nuw %dataframe, ptr %df51, i32 0, i32 0
  store ptr %arr_header35, ptr %21, align 8
  %22 = getelementptr inbounds nuw %dataframe, ptr %df51, i32 0, i32 1
  store ptr %arr_header42, ptr %22, align 8
  %23 = getelementptr inbounds nuw %dataframe, ptr %df51, i32 0, i32 2
  store ptr %arr_header44, ptr %23, align 8
  store ptr %df51, ptr @__where_result, align 8
  store i64 0, ptr @__where_i, align 8
  br label %for.cond52

idx_ok:                                           ; preds = %for.body
  %elem_ptr16 = getelementptr ptr, ptr %data_ptr, i64 %__where_i_load15
  %record = load ptr, ptr %elem_ptr16, align 8
  %ptr_savings = getelementptr ptr, ptr %record, i64 4
  %load_savings_ptr = load ptr, ptr %ptr_savings, align 8
  %val_savings = load double, ptr %load_savings_ptr, align 8
  %fcmp_tmp = fcmp ogt double %val_savings, 0x41252988F0A3D70A
  br i1 %fcmp_tmp, label %then, label %else

idx_err:                                          ; preds = %for.body
  ret ptr null

then:                                             ; preds = %idx_ok
  %__where_result_load = load ptr, ptr @__where_result, align 8
  %__where_src_load17 = load ptr, ptr @__where_src, align 8
  %__where_i_load18 = load i64, ptr @__where_i, align 8
  %rows_ptr_ptr19 = getelementptr inbounds nuw %dataframe, ptr %__where_src_load17, i32 0, i32 1
  %rows20 = load ptr, ptr %rows_ptr_ptr19, align 8
  %data_ptr_ptr21 = getelementptr inbounds nuw %array, ptr %rows20, i32 0, i32 2
  %data_ptr22 = load ptr, ptr %data_ptr_ptr21, align 8
  %len_ptr23 = getelementptr inbounds nuw %array, ptr %rows20, i32 0, i32 0
  %len24 = load i64, ptr %len_ptr23, align 4
  %in_bounds25 = icmp ult i64 %__where_i_load18, %len24
  br i1 %in_bounds25, label %idx_ok26, label %idx_err27

else:                                             ; preds = %idx_ok
  br label %ifcont

ifcont:                                           ; preds = %else, %add_cont
  %iftmp = phi ptr [ %__where_result_load, %add_cont ], [ 0.000000e+00, %else ]
  br label %for.step

idx_ok26:                                         ; preds = %then
  %elem_ptr28 = getelementptr ptr, ptr %data_ptr22, i64 %__where_i_load18
  %record29 = load ptr, ptr %elem_ptr28, align 8
  %rows_field = getelementptr inbounds nuw { ptr, ptr, ptr }, ptr %__where_result_load, i32 0, i32 1
  %rows_array = load ptr, ptr %rows_field, align 8
  %len_ptr30 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %rows_array, i32 0, i32 0
  %cap_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %rows_array, i32 0, i32 1
  %data_ptr_ptr31 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %rows_array, i32 0, i32 2
  %len32 = load i64, ptr %len_ptr30, align 4
  %cap = load i64, ptr %cap_ptr, align 4
  %data_ptr33 = load ptr, ptr %data_ptr_ptr31, align 8
  %is_full = icmp uge i64 %len32, %cap
  br i1 %is_full, label %grow, label %add_cont

idx_err27:                                        ; preds = %then
  ret ptr null

grow:                                             ; preds = %idx_ok26
  %24 = icmp eq i64 %cap, 0
  %25 = mul i64 %cap, 1000
  %new_cap = select i1 %24, i64 4, i64 %25
  %new_byte_size = mul i64 %new_cap, 8
  %realloc_ptr = call ptr @realloc(ptr %data_ptr33, i64 %new_byte_size)
  store i64 %new_cap, ptr %cap_ptr, align 4
  store ptr %realloc_ptr, ptr %data_ptr_ptr31, align 8
  br label %add_cont

add_cont:                                         ; preds = %grow, %idx_ok26
  %final_data_ptr = phi ptr [ %data_ptr33, %idx_ok26 ], [ %realloc_ptr, %grow ]
  %target_ptr = getelementptr ptr, ptr %final_data_ptr, i64 %len32
  store ptr %record29, ptr %target_ptr, align 8
  %next_len = add i64 %len32, 1
  store i64 %next_len, ptr %len_ptr30, align 4
  br label %ifcont

for.cond52:                                       ; preds = %for.step54, %for.end
  %__where_i_load56 = load i64, ptr @__where_i, align 8
  %__where_src_load57 = load ptr, ptr @__where_src, align 8
  %rows_ptr_field58 = getelementptr inbounds nuw { ptr, ptr, ptr }, ptr %__where_src_load57, i32 0, i32 1
  %rows_ptr59 = load ptr, ptr %rows_ptr_field58, align 8
  %rows_array_ptr60 = bitcast ptr %rows_ptr59 to ptr
  %rows_length_ptr61 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %rows_array_ptr60, i32 0, i32 0
  %rows_length62 = load i64, ptr %rows_length_ptr61, align 8
  %icmp_tmp63 = icmp slt i64 %__where_i_load56, %rows_length62
  br i1 %icmp_tmp63, label %for.body53, label %for.end55

for.body53:                                       ; preds = %for.cond52
  %__where_src_load64 = load ptr, ptr @__where_src, align 8
  %__where_i_load65 = load i64, ptr @__where_i, align 8
  %rows_ptr_ptr66 = getelementptr inbounds nuw %dataframe, ptr %__where_src_load64, i32 0, i32 1
  %rows67 = load ptr, ptr %rows_ptr_ptr66, align 8
  %data_ptr_ptr68 = getelementptr inbounds nuw %array, ptr %rows67, i32 0, i32 2
  %data_ptr69 = load ptr, ptr %data_ptr_ptr68, align 8
  %len_ptr70 = getelementptr inbounds nuw %array, ptr %rows67, i32 0, i32 0
  %len71 = load i64, ptr %len_ptr70, align 4
  %in_bounds72 = icmp ult i64 %__where_i_load65, %len71
  br i1 %in_bounds72, label %idx_ok73, label %idx_err74

for.step54:                                       ; preds = %ifcont96
  %inc_load129 = load i64, ptr @__where_i, align 4
  %inc_add130 = add i64 %inc_load129, 1
  store i64 %inc_add130, ptr @__where_i, align 8
  br label %for.cond52

for.end55:                                        ; preds = %for.cond52
  %__where_result_load131 = load ptr, ptr @__where_result, align 8
  %runtime_obj = call ptr @malloc(i64 16)
  %runtime_cast = bitcast ptr %runtime_obj to ptr
  %tag_ptr = getelementptr inbounds nuw { i16, [6 x i8], ptr }, ptr %runtime_cast, i32 0, i32 0
  store i16 7, ptr %tag_ptr, align 8
  %data_ptr132 = getelementptr inbounds nuw { i16, [6 x i8], ptr }, ptr %runtime_cast, i32 0, i32 2
  store ptr %__where_result_load131, ptr %data_ptr132, align 8
  ret ptr %runtime_obj

idx_ok73:                                         ; preds = %for.body53
  %elem_ptr75 = getelementptr ptr, ptr %data_ptr69, i64 %__where_i_load65
  %record76 = load ptr, ptr %elem_ptr75, align 8
  %ptr_savings77 = getelementptr ptr, ptr %record76, i64 4
  %load_savings_ptr78 = load ptr, ptr %ptr_savings77, align 8
  %val_savings79 = load double, ptr %load_savings_ptr78, align 8
  %fcmp_tmp80 = fcmp olt double %val_savings79, 0x415A55A51E147AE1
  %__where_src_load81 = load ptr, ptr @__where_src, align 8
  %__where_i_load82 = load i64, ptr @__where_i, align 8
  %rows_ptr_ptr83 = getelementptr inbounds nuw %dataframe, ptr %__where_src_load81, i32 0, i32 1
  %rows84 = load ptr, ptr %rows_ptr_ptr83, align 8
  %data_ptr_ptr85 = getelementptr inbounds nuw %array, ptr %rows84, i32 0, i32 2
  %data_ptr86 = load ptr, ptr %data_ptr_ptr85, align 8
  %len_ptr87 = getelementptr inbounds nuw %array, ptr %rows84, i32 0, i32 0
  %len88 = load i64, ptr %len_ptr87, align 4
  %in_bounds89 = icmp ult i64 %__where_i_load82, %len88
  br i1 %in_bounds89, label %idx_ok90, label %idx_err91

idx_err74:                                        ; preds = %for.body53
  ret ptr null

idx_ok90:                                         ; preds = %idx_ok73
  %elem_ptr92 = getelementptr ptr, ptr %data_ptr86, i64 %__where_i_load82
  %record93 = load ptr, ptr %elem_ptr92, align 8
  %ptr_name = getelementptr ptr, ptr %record93, i64 1
  %load_name_ptr = load ptr, ptr %ptr_name, align 8
  %strcmp_res = call i32 @strcmp(ptr %load_name_ptr, ptr @str.10)
  %str_eq = icmp eq i32 %strcmp_res, 0
  %andtmp = and i1 %fcmp_tmp80, %str_eq
  br i1 %andtmp, label %then94, label %else95

idx_err91:                                        ; preds = %idx_ok73
  ret ptr null

then94:                                           ; preds = %idx_ok90
  %__where_result_load97 = load ptr, ptr @__where_result, align 8
  %__where_src_load98 = load ptr, ptr @__where_src, align 8
  %__where_i_load99 = load i64, ptr @__where_i, align 8
  %rows_ptr_ptr100 = getelementptr inbounds nuw %dataframe, ptr %__where_src_load98, i32 0, i32 1
  %rows101 = load ptr, ptr %rows_ptr_ptr100, align 8
  %data_ptr_ptr102 = getelementptr inbounds nuw %array, ptr %rows101, i32 0, i32 2
  %data_ptr103 = load ptr, ptr %data_ptr_ptr102, align 8
  %len_ptr104 = getelementptr inbounds nuw %array, ptr %rows101, i32 0, i32 0
  %len105 = load i64, ptr %len_ptr104, align 4
  %in_bounds106 = icmp ult i64 %__where_i_load99, %len105
  br i1 %in_bounds106, label %idx_ok107, label %idx_err108

else95:                                           ; preds = %idx_ok90
  br label %ifcont96

ifcont96:                                         ; preds = %else95, %add_cont121
  %iftmp128 = phi ptr [ %__where_result_load97, %add_cont121 ], [ 0.000000e+00, %else95 ]
  br label %for.step54

idx_ok107:                                        ; preds = %then94
  %elem_ptr109 = getelementptr ptr, ptr %data_ptr103, i64 %__where_i_load99
  %record110 = load ptr, ptr %elem_ptr109, align 8
  %rows_field111 = getelementptr inbounds nuw { ptr, ptr, ptr }, ptr %__where_result_load97, i32 0, i32 1
  %rows_array112 = load ptr, ptr %rows_field111, align 8
  %len_ptr113 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %rows_array112, i32 0, i32 0
  %cap_ptr114 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %rows_array112, i32 0, i32 1
  %data_ptr_ptr115 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %rows_array112, i32 0, i32 2
  %len116 = load i64, ptr %len_ptr113, align 4
  %cap117 = load i64, ptr %cap_ptr114, align 4
  %data_ptr118 = load ptr, ptr %data_ptr_ptr115, align 8
  %is_full119 = icmp uge i64 %len116, %cap117
  br i1 %is_full119, label %grow120, label %add_cont121

idx_err108:                                       ; preds = %then94
  ret ptr null

grow120:                                          ; preds = %idx_ok107
  %26 = icmp eq i64 %cap117, 0
  %27 = mul i64 %cap117, 1000
  %new_cap122 = select i1 %26, i64 4, i64 %27
  %new_byte_size123 = mul i64 %new_cap122, 8
  %realloc_ptr124 = call ptr @realloc(ptr %data_ptr118, i64 %new_byte_size123)
  store i64 %new_cap122, ptr %cap_ptr114, align 4
  store ptr %realloc_ptr124, ptr %data_ptr_ptr115, align 8
  br label %add_cont121

add_cont121:                                      ; preds = %grow120, %idx_ok107
  %final_data_ptr125 = phi ptr [ %data_ptr118, %idx_ok107 ], [ %realloc_ptr124, %grow120 ]
  %target_ptr126 = getelementptr ptr, ptr %final_data_ptr125, i64 %len116
  store ptr %record110, ptr %target_ptr126, align 8
  %next_len127 = add i64 %len116, 1
  store i64 %next_len127, ptr %len_ptr113, align 4
  br label %ifcont96
}

declare i32 @printf(ptr, ...)

declare noalias ptr @malloc(i64)

declare ptr @realloc(ptr, i64)

declare i32 @strcmp(ptr, ptr)
