; ModuleID = 'repl_module'
source_filename = "repl_module"

%dataframe = type { ptr, ptr, ptr }
%array = type { i64, i64, ptr }

@x = external global ptr
@__where_src = external global ptr, align 8
@str = private unnamed_addr constant [5 x i8] c"name\00", align 1
@str.1 = private unnamed_addr constant [4 x i8] c"age\00", align 1
@__where_result = external global ptr, align 8
@__where_i = external global i64, align 8
@str.2 = private unnamed_addr constant [5 x i8] c"name\00", align 1
@str.3 = private unnamed_addr constant [4 x i8] c"age\00", align 1
@str.4 = private unnamed_addr constant [12 x i8] c"Hary potter\00", align 1

define ptr @main_10() {
entry:
  %x_load = load ptr, ptr @x, align 8
  store ptr %x_load, ptr @__where_src, align 8
  %arr_header = call ptr @malloc(i64 24)
  %arr_data = call ptr @malloc(i64 320000)
  %0 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header, i32 0, i32 0
  store i64 2, ptr %0, align 4
  %1 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header, i32 0, i32 1
  store i64 40000, ptr %1, align 4
  %2 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header, i32 0, i32 2
  store ptr %arr_data, ptr %2, align 8
  %elem_ptr = getelementptr ptr, ptr %arr_data, i64 0
  store ptr @str, ptr %elem_ptr, align 8
  %elem_ptr1 = getelementptr ptr, ptr %arr_data, i64 1
  store ptr @str.1, ptr %elem_ptr1, align 8
  %arr_header2 = call ptr @malloc(i64 24)
  %arr_data3 = call ptr @malloc(i64 800)
  %3 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header2, i32 0, i32 0
  store i64 0, ptr %3, align 4
  %4 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header2, i32 0, i32 1
  store i64 100, ptr %4, align 4
  %5 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header2, i32 0, i32 2
  store ptr %arr_data3, ptr %5, align 8
  %arr_header4 = call ptr @malloc(i64 24)
  %arr_data5 = call ptr @malloc(i64 320000)
  %6 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header4, i32 0, i32 0
  store i64 2, ptr %6, align 4
  %7 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header4, i32 0, i32 1
  store i64 40000, ptr %7, align 4
  %8 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header4, i32 0, i32 2
  store ptr %arr_data5, ptr %8, align 8
  %elem_ptr6 = getelementptr ptr, ptr %arr_data5, i64 0
  store ptr bitcast (i64 4 to ptr), ptr %elem_ptr6, align 8
  %elem_ptr7 = getelementptr ptr, ptr %arr_data5, i64 1
  store ptr bitcast (i64 1 to ptr), ptr %elem_ptr7, align 8
  %df = tail call ptr @malloc(i32 ptrtoint (ptr getelementptr (%dataframe, ptr null, i32 1) to i32))
  %9 = getelementptr inbounds nuw %dataframe, ptr %df, i32 0, i32 0
  store ptr %arr_header, ptr %9, align 8
  %10 = getelementptr inbounds nuw %dataframe, ptr %df, i32 0, i32 1
  store ptr %arr_header2, ptr %10, align 8
  %11 = getelementptr inbounds nuw %dataframe, ptr %df, i32 0, i32 2
  store ptr %arr_header4, ptr %11, align 8
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
  %__where_src_load8 = load ptr, ptr @__where_src, align 8
  %__where_i_load9 = load i64, ptr @__where_i, align 8
  %rows_ptr_ptr = getelementptr inbounds nuw %dataframe, ptr %__where_src_load8, i32 0, i32 1
  %rows = load ptr, ptr %rows_ptr_ptr, align 8
  %data_ptr_ptr = getelementptr inbounds nuw %array, ptr %rows, i32 0, i32 2
  %data_ptr = load ptr, ptr %data_ptr_ptr, align 8
  %len_ptr = getelementptr inbounds nuw %array, ptr %rows, i32 0, i32 0
  %len = load i64, ptr %len_ptr, align 4
  %in_bounds = icmp ult i64 %__where_i_load9, %len
  br i1 %in_bounds, label %idx_ok, label %idx_err

for.step:                                         ; preds = %ifcont
  %inc_load = load i64, ptr @__where_i, align 4
  %inc_add = add i64 %inc_load, 1
  store i64 %inc_add, ptr @__where_i, align 8
  br label %for.cond

for.end:                                          ; preds = %for.cond
  %__where_result_load29 = load ptr, ptr @__where_result, align 8
  store ptr %__where_result_load29, ptr @__where_src, align 8
  %arr_header30 = call ptr @malloc(i64 24)
  %arr_data31 = call ptr @malloc(i64 320000)
  %12 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header30, i32 0, i32 0
  store i64 2, ptr %12, align 4
  %13 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header30, i32 0, i32 1
  store i64 40000, ptr %13, align 4
  %14 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header30, i32 0, i32 2
  store ptr %arr_data31, ptr %14, align 8
  %elem_ptr32 = getelementptr ptr, ptr %arr_data31, i64 0
  store ptr @str.2, ptr %elem_ptr32, align 8
  %elem_ptr33 = getelementptr ptr, ptr %arr_data31, i64 1
  store ptr @str.3, ptr %elem_ptr33, align 8
  %arr_header34 = call ptr @malloc(i64 24)
  %arr_data35 = call ptr @malloc(i64 800)
  %15 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header34, i32 0, i32 0
  store i64 0, ptr %15, align 4
  %16 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header34, i32 0, i32 1
  store i64 100, ptr %16, align 4
  %17 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header34, i32 0, i32 2
  store ptr %arr_data35, ptr %17, align 8
  %arr_header36 = call ptr @malloc(i64 24)
  %arr_data37 = call ptr @malloc(i64 320000)
  %18 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header36, i32 0, i32 0
  store i64 2, ptr %18, align 4
  %19 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header36, i32 0, i32 1
  store i64 40000, ptr %19, align 4
  %20 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header36, i32 0, i32 2
  store ptr %arr_data37, ptr %20, align 8
  %elem_ptr38 = getelementptr ptr, ptr %arr_data37, i64 0
  store ptr bitcast (i64 4 to ptr), ptr %elem_ptr38, align 8
  %elem_ptr39 = getelementptr ptr, ptr %arr_data37, i64 1
  store ptr bitcast (i64 1 to ptr), ptr %elem_ptr39, align 8
  %df40 = tail call ptr @malloc(i32 ptrtoint (ptr getelementptr (%dataframe, ptr null, i32 1) to i32))
  %21 = getelementptr inbounds nuw %dataframe, ptr %df40, i32 0, i32 0
  store ptr %arr_header30, ptr %21, align 8
  %22 = getelementptr inbounds nuw %dataframe, ptr %df40, i32 0, i32 1
  store ptr %arr_header34, ptr %22, align 8
  %23 = getelementptr inbounds nuw %dataframe, ptr %df40, i32 0, i32 2
  store ptr %arr_header36, ptr %23, align 8
  store ptr %df40, ptr @__where_result, align 8
  store i64 0, ptr @__where_i, align 8
  br label %for.cond41

idx_ok:                                           ; preds = %for.body
  %elem_ptr10 = getelementptr ptr, ptr %data_ptr, i64 %__where_i_load9
  %record = load ptr, ptr %elem_ptr10, align 8
  %ptr_age = getelementptr ptr, ptr %record, i64 1
  %load_age_ptr = load ptr, ptr %ptr_age, align 8
  %val_age = load i64, ptr %load_age_ptr, align 4
  %icmp_tmp11 = icmp sgt i64 %val_age, 91
  br i1 %icmp_tmp11, label %then, label %else

idx_err:                                          ; preds = %for.body
  ret ptr null

then:                                             ; preds = %idx_ok
  %__where_result_load = load ptr, ptr @__where_result, align 8
  %__where_src_load12 = load ptr, ptr @__where_src, align 8
  %__where_i_load13 = load i64, ptr @__where_i, align 8
  %rows_ptr_ptr14 = getelementptr inbounds nuw %dataframe, ptr %__where_src_load12, i32 0, i32 1
  %rows15 = load ptr, ptr %rows_ptr_ptr14, align 8
  %data_ptr_ptr16 = getelementptr inbounds nuw %array, ptr %rows15, i32 0, i32 2
  %data_ptr17 = load ptr, ptr %data_ptr_ptr16, align 8
  %len_ptr18 = getelementptr inbounds nuw %array, ptr %rows15, i32 0, i32 0
  %len19 = load i64, ptr %len_ptr18, align 4
  %in_bounds20 = icmp ult i64 %__where_i_load13, %len19
  br i1 %in_bounds20, label %idx_ok21, label %idx_err22

else:                                             ; preds = %idx_ok
  br label %ifcont

ifcont:                                           ; preds = %else, %add_cont
  %iftmp = phi ptr [ %__where_result_load, %add_cont ], [ 0.000000e+00, %else ]
  br label %for.step

idx_ok21:                                         ; preds = %then
  %elem_ptr23 = getelementptr ptr, ptr %data_ptr17, i64 %__where_i_load13
  %record24 = load ptr, ptr %elem_ptr23, align 8
  %rows_field = getelementptr inbounds nuw { ptr, ptr, ptr }, ptr %__where_result_load, i32 0, i32 1
  %rows_array = load ptr, ptr %rows_field, align 8
  %len_ptr25 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %rows_array, i32 0, i32 0
  %cap_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %rows_array, i32 0, i32 1
  %data_ptr_ptr26 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %rows_array, i32 0, i32 2
  %len27 = load i64, ptr %len_ptr25, align 4
  %cap = load i64, ptr %cap_ptr, align 4
  %data_ptr28 = load ptr, ptr %data_ptr_ptr26, align 8
  %is_full = icmp uge i64 %len27, %cap
  br i1 %is_full, label %grow, label %add_cont

idx_err22:                                        ; preds = %then
  ret ptr null

grow:                                             ; preds = %idx_ok21
  %24 = icmp eq i64 %cap, 0
  %25 = mul i64 %cap, 1000
  %new_cap = select i1 %24, i64 4, i64 %25
  %new_byte_size = mul i64 %new_cap, 8
  %realloc_ptr = call ptr @realloc(ptr %data_ptr28, i64 %new_byte_size)
  store i64 %new_cap, ptr %cap_ptr, align 4
  store ptr %realloc_ptr, ptr %data_ptr_ptr26, align 8
  br label %add_cont

add_cont:                                         ; preds = %grow, %idx_ok21
  %final_data_ptr = phi ptr [ %data_ptr28, %idx_ok21 ], [ %realloc_ptr, %grow ]
  %target_ptr = getelementptr ptr, ptr %final_data_ptr, i64 %len27
  store ptr %record24, ptr %target_ptr, align 8
  %next_len = add i64 %len27, 1
  store i64 %next_len, ptr %len_ptr25, align 4
  br label %ifcont

for.cond41:                                       ; preds = %for.step43, %for.end
  %__where_i_load45 = load i64, ptr @__where_i, align 8
  %__where_src_load46 = load ptr, ptr @__where_src, align 8
  %rows_ptr_field47 = getelementptr inbounds nuw { ptr, ptr, ptr }, ptr %__where_src_load46, i32 0, i32 1
  %rows_ptr48 = load ptr, ptr %rows_ptr_field47, align 8
  %rows_array_ptr49 = bitcast ptr %rows_ptr48 to ptr
  %rows_length_ptr50 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %rows_array_ptr49, i32 0, i32 0
  %rows_length51 = load i64, ptr %rows_length_ptr50, align 8
  %icmp_tmp52 = icmp slt i64 %__where_i_load45, %rows_length51
  br i1 %icmp_tmp52, label %for.body42, label %for.end44

for.body42:                                       ; preds = %for.cond41
  %__where_src_load53 = load ptr, ptr @__where_src, align 8
  %__where_i_load54 = load i64, ptr @__where_i, align 8
  %rows_ptr_ptr55 = getelementptr inbounds nuw %dataframe, ptr %__where_src_load53, i32 0, i32 1
  %rows56 = load ptr, ptr %rows_ptr_ptr55, align 8
  %data_ptr_ptr57 = getelementptr inbounds nuw %array, ptr %rows56, i32 0, i32 2
  %data_ptr58 = load ptr, ptr %data_ptr_ptr57, align 8
  %len_ptr59 = getelementptr inbounds nuw %array, ptr %rows56, i32 0, i32 0
  %len60 = load i64, ptr %len_ptr59, align 4
  %in_bounds61 = icmp ult i64 %__where_i_load54, %len60
  br i1 %in_bounds61, label %idx_ok62, label %idx_err63

for.step43:                                       ; preds = %ifcont85
  %inc_load118 = load i64, ptr @__where_i, align 4
  %inc_add119 = add i64 %inc_load118, 1
  store i64 %inc_add119, ptr @__where_i, align 8
  br label %for.cond41

for.end44:                                        ; preds = %for.cond41
  %__where_result_load120 = load ptr, ptr @__where_result, align 8
  %runtime_obj = call ptr @malloc(i64 16)
  %runtime_cast = bitcast ptr %runtime_obj to ptr
  %tag_ptr = getelementptr inbounds nuw { i16, [6 x i8], ptr }, ptr %runtime_cast, i32 0, i32 0
  store i16 7, ptr %tag_ptr, align 8
  %data_ptr121 = getelementptr inbounds nuw { i16, [6 x i8], ptr }, ptr %runtime_cast, i32 0, i32 2
  store ptr %__where_result_load120, ptr %data_ptr121, align 8
  ret ptr %runtime_obj

idx_ok62:                                         ; preds = %for.body42
  %elem_ptr64 = getelementptr ptr, ptr %data_ptr58, i64 %__where_i_load54
  %record65 = load ptr, ptr %elem_ptr64, align 8
  %ptr_age66 = getelementptr ptr, ptr %record65, i64 1
  %load_age_ptr67 = load ptr, ptr %ptr_age66, align 8
  %val_age68 = load i64, ptr %load_age_ptr67, align 4
  %icmp_tmp69 = icmp slt i64 %val_age68, 93
  %__where_src_load70 = load ptr, ptr @__where_src, align 8
  %__where_i_load71 = load i64, ptr @__where_i, align 8
  %rows_ptr_ptr72 = getelementptr inbounds nuw %dataframe, ptr %__where_src_load70, i32 0, i32 1
  %rows73 = load ptr, ptr %rows_ptr_ptr72, align 8
  %data_ptr_ptr74 = getelementptr inbounds nuw %array, ptr %rows73, i32 0, i32 2
  %data_ptr75 = load ptr, ptr %data_ptr_ptr74, align 8
  %len_ptr76 = getelementptr inbounds nuw %array, ptr %rows73, i32 0, i32 0
  %len77 = load i64, ptr %len_ptr76, align 4
  %in_bounds78 = icmp ult i64 %__where_i_load71, %len77
  br i1 %in_bounds78, label %idx_ok79, label %idx_err80

idx_err63:                                        ; preds = %for.body42
  ret ptr null

idx_ok79:                                         ; preds = %idx_ok62
  %elem_ptr81 = getelementptr ptr, ptr %data_ptr75, i64 %__where_i_load71
  %record82 = load ptr, ptr %elem_ptr81, align 8
  %ptr_name = getelementptr ptr, ptr %record82, i64 0
  %load_name_ptr = load ptr, ptr %ptr_name, align 8
  %strcmp_res = call i32 @strcmp(ptr %load_name_ptr, ptr @str.4)
  %str_eq = icmp eq i32 %strcmp_res, 0
  %andtmp = and i1 %icmp_tmp69, %str_eq
  br i1 %andtmp, label %then83, label %else84

idx_err80:                                        ; preds = %idx_ok62
  ret ptr null

then83:                                           ; preds = %idx_ok79
  %__where_result_load86 = load ptr, ptr @__where_result, align 8
  %__where_src_load87 = load ptr, ptr @__where_src, align 8
  %__where_i_load88 = load i64, ptr @__where_i, align 8
  %rows_ptr_ptr89 = getelementptr inbounds nuw %dataframe, ptr %__where_src_load87, i32 0, i32 1
  %rows90 = load ptr, ptr %rows_ptr_ptr89, align 8
  %data_ptr_ptr91 = getelementptr inbounds nuw %array, ptr %rows90, i32 0, i32 2
  %data_ptr92 = load ptr, ptr %data_ptr_ptr91, align 8
  %len_ptr93 = getelementptr inbounds nuw %array, ptr %rows90, i32 0, i32 0
  %len94 = load i64, ptr %len_ptr93, align 4
  %in_bounds95 = icmp ult i64 %__where_i_load88, %len94
  br i1 %in_bounds95, label %idx_ok96, label %idx_err97

else84:                                           ; preds = %idx_ok79
  br label %ifcont85

ifcont85:                                         ; preds = %else84, %add_cont110
  %iftmp117 = phi ptr [ %__where_result_load86, %add_cont110 ], [ 0.000000e+00, %else84 ]
  br label %for.step43

idx_ok96:                                         ; preds = %then83
  %elem_ptr98 = getelementptr ptr, ptr %data_ptr92, i64 %__where_i_load88
  %record99 = load ptr, ptr %elem_ptr98, align 8
  %rows_field100 = getelementptr inbounds nuw { ptr, ptr, ptr }, ptr %__where_result_load86, i32 0, i32 1
  %rows_array101 = load ptr, ptr %rows_field100, align 8
  %len_ptr102 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %rows_array101, i32 0, i32 0
  %cap_ptr103 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %rows_array101, i32 0, i32 1
  %data_ptr_ptr104 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %rows_array101, i32 0, i32 2
  %len105 = load i64, ptr %len_ptr102, align 4
  %cap106 = load i64, ptr %cap_ptr103, align 4
  %data_ptr107 = load ptr, ptr %data_ptr_ptr104, align 8
  %is_full108 = icmp uge i64 %len105, %cap106
  br i1 %is_full108, label %grow109, label %add_cont110

idx_err97:                                        ; preds = %then83
  ret ptr null

grow109:                                          ; preds = %idx_ok96
  %26 = icmp eq i64 %cap106, 0
  %27 = mul i64 %cap106, 1000
  %new_cap111 = select i1 %26, i64 4, i64 %27
  %new_byte_size112 = mul i64 %new_cap111, 8
  %realloc_ptr113 = call ptr @realloc(ptr %data_ptr107, i64 %new_byte_size112)
  store i64 %new_cap111, ptr %cap_ptr103, align 4
  store ptr %realloc_ptr113, ptr %data_ptr_ptr104, align 8
  br label %add_cont110

add_cont110:                                      ; preds = %grow109, %idx_ok96
  %final_data_ptr114 = phi ptr [ %data_ptr107, %idx_ok96 ], [ %realloc_ptr113, %grow109 ]
  %target_ptr115 = getelementptr ptr, ptr %final_data_ptr114, i64 %len105
  store ptr %record99, ptr %target_ptr115, align 8
  %next_len116 = add i64 %len105, 1
  store i64 %next_len116, ptr %len_ptr102, align 4
  br label %ifcont85
}

declare i32 @printf(ptr, ...)

declare noalias ptr @malloc(i64)

declare ptr @realloc(ptr, i64)

declare i32 @strcmp(ptr, ptr)
